using Paillave.Etl.Core;
using Paillave.Etl.EntityFrameworkCore;
using Paillave.Etl.Samples.DataAccess;
using Paillave.Etl.TextFile;

namespace Paillave.Etl.Samples
{
    public static class TestImport2
    {
        public static void Import(ISingleStream<string[]> contextStream)
        {
            contextStream.EfCoreSelect("erre", (o, i) => o.Set<Portfolio>().Select(i => new Portfolio { InternalCode = i.InternalCode, Name = i.Name, SicavId = i.SicavId }));
            var portfolioFileStream = contextStream
                .FromConnector("Get portfolio files", "PTF")
                .CrossApplyTextFile("Parse portfolio file", FlatFileDefinition.Create(i => new
                {
                    SicavCode = i.ToColumn("SicavCode"),
                    SicavName = i.ToColumn("SicavName"),
                    SicavType = i.ToColumn("SicavType"),
                    PortfolioCode = i.ToColumn("PortfolioCode"),
                    PortfolioName = i.ToColumn("PortfolioName")
                }).IsColumnSeparated(','))
                .SetForCorrelation("Correlate portfolio row");

            var positionFileStream = contextStream
                .FromConnector("Get position files", "POS")
                .CrossApplyTextFile("Parse position file", FlatFileDefinition.Create(i => new
                {
                    PortfolioCode = i.ToColumn("PortfolioCode"),
                    SecurityCode = i.ToColumn("SecurityCode"),
                    Isin = i.ToColumn("Isin"),
                    SecurityName = i.ToColumn("SecurityName"),
                    SecurityClass = i.ToColumn("SecurityClass"),
                    Issuer = i.ToColumn("Issuer"),
                    Date = i.ToDateColumn("Date", "yyyyMMdd"),
                    Value = i.ToNumberColumn<decimal>("Value", "."),
                }).IsColumnSeparated(','))
                .SetForCorrelation("Correlate position row");

            var sicavStream = portfolioFileStream
                .Distinct("Distinct sicav", i => i.SicavCode, true)
                .Select("Create sicav", i => new DataAccess.Sicav
                {
                    InternalCode = i.SicavCode,
                    Name = i.SicavName,
                    Type = i.SicavType == "UCITS" ? DataAccess.SicavType.UCITS : DataAccess.SicavType.AIFM
                })
                .EfCoreSave("Save sicav", o => o
                    .SeekOn(i => i.InternalCode)
                    // .DoNotUpdateIfExists()
                    .WithMode(SaveMode.EntityFrameworkCore));

            var portfolioStream = portfolioFileStream
                .Distinct("Distinct portfolio", i => i.PortfolioCode, true)
                .CorrelateToSingle("Get related sicav and create portfolio", sicavStream, (row, sicav) => new DataAccess.Portfolio
                {
                    InternalCode = row.PortfolioCode,
                    Name = row.PortfolioName,
                    SicavId = sicav.Id
                })
                .EfCoreSave("Save portfolio", o => o
                    .SeekOn(i => i.InternalCode)
                    .DoNotUpdateIfExists());

            var compositionStream = positionFileStream
                .Distinct("Distinct compositions", i => new { i.PortfolioCode, i.Date }, true)
                .Lookup("Get related portfolio",
                    portfolioStream,
                    i => i.PortfolioCode,
                    i => i.InternalCode,
                    (row, portfolio) => new
                    {
                        Portfolio = portfolio,
                        Composition = new DataAccess.Composition
                        {
                            Date = row.Date,
                            PortfolioId = portfolio.Id
                        }
                    })
                .EfCoreSave("Save composition", o => o
                    .Entity(i => i.Composition)
                    .SeekOn(i => new { i.Date, i.PortfolioId })
                    .DoNotUpdateIfExists()
                    .Output((i, e) => new
                    {
                        i.Portfolio,
                        Composition = e
                    }));

            var securityStream = positionFileStream
                .Distinct("Distinct securities", i => i.SecurityCode)
                .Select("Create security", i =>
                {
                    if (string.IsNullOrWhiteSpace(i.SecurityClass))
                    {
                        return new DataAccess.Equity
                        {
                            InternalCode = i.SecurityCode,
                            Name = i.SecurityName,
                            Isin = i.Isin,
                            Issuer = i.Issuer
                        } as DataAccess.Security;
                    }
                    return new DataAccess.ShareClass
                    {
                        InternalCode = i.SecurityCode,
                        Name = i.SecurityName,
                        Isin = i.Isin,
                        Class = i.SecurityClass
                    } as DataAccess.Security;
                })
                .EfCoreSave("Save security", o => o
                    .SeekOn(i => i.Isin)
                    .AlternativelySeekOn(i => i.InternalCode)
                    .DoNotUpdateIfExists());

            positionFileStream
                .CorrelateToSingle("Get related security", securityStream, (row, security) => new { Row = row, SecurityId = security.Id })
                .CorrelateToSingle("Get related composition and create position", compositionStream, (row, composition) => new DataAccess.Position
                {
                    Value = row.Row.Value,
                    SecurityId = row.SecurityId,
                    CompositionId = composition.Composition.Id
                })
                .Distinct("Distinct positions", i => new { i.CompositionId, i.SecurityId }, o => o.ForProperty(i => i.Value, DistinctAggregator.Sum))
                .EfCoreSave("Save position")
                .CorrelateToSingle("Get position portfolio", compositionStream, (p, c) => new { c.Portfolio.InternalCode, c.Composition.Date, p.Value })
                .Distinct("Aggregate position per portfolio", i => new { i.InternalCode, i.Date }, o => o.ForProperty(i => i.Value, DistinctAggregator.Sum))
                .ToTextFileValue("Export portfolios weight", "PortfoliosWeight.csv", FlatFileDefinition.Create(i => new
                {
                    InternalCode = i.ToColumn("Code"),
                    Date = i.ToDateColumn("Date", "ddd dd MMM, yyyy"),
                    Value = i.ToNumberColumn<decimal>("Weight", "."),
                }).IsColumnSeparated(','))
                .ToConnector("Save portfolios position file", "OUT");
        }
    }
}