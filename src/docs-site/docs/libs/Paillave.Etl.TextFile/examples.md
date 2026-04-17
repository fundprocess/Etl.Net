---
title: Examples
---

## Parse CSV and write summary

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.TextFile;

var runner = StreamProcessRunner.Create<string>(s => s
    .CrossApplyFolderFiles("list csv", "*.csv")
    .CrossApplyTextFile("parse", FlatFileDefinition.Create(i => new
    {
        Email = i.ToColumn("email"),
        Reputation = i.ToNumberColumn<int?>("reputation", ".")
    }).IsColumnSeparated(','))
    .Distinct("unique", i => i.Email)
    .Select("summary", i => new { i.Email, i.Reputation })
    .ToTextFileValue("to csv", "summary.csv", FlatFileDefinition.Create(i => new
    {
        Email = i.ToColumn("Email"),
        Reputation = i.ToNumberColumn<int?>("Reputation", ".")
    }).IsColumnSeparated(','))
    .WriteToFile("write", f => $"/tmp/{f.Name}"));

await runner.ExecuteAsync("/data/csv", new ExecutionOptions<string>());
```
