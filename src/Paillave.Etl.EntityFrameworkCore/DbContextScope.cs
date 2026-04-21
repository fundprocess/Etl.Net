using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Paillave.Etl.EntityFrameworkCore;

internal sealed class DbContextScope : IDisposable
{
    public DbContext Context { get; }

    private DbContextScope(DbContext context) => Context = context;

    public static DbContextScope Create(IServiceProvider services, string? key = null, Type? dbContextType = null)
    {
        DbContext? ctx = key != null
            ? services.GetKeyedService<DbContext>(key)
            : dbContextType != null
                ? services.GetService(dbContextType) as DbContext
                : services.GetService<DbContext>();

        return new DbContextScope(ctx ?? throw new InvalidOperationException(
            $"No DbContext could be resolved. Check your dependency injection configuration."));
    }

    public void Dispose() => Context.Dispose();
}
