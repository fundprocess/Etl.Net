using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Paillave.Etl.EntityFrameworkCore;

/// <summary>
/// Wraps a DI scope and the DbContext resolved from it,
/// ensuring each resolution gets a fresh DbContext instance
/// even when registered as Scoped (AddDbContext / AddDbContextPool).
/// Disposing this object disposes both the scope and the context.
/// </summary>
internal sealed class DbContextScope : IDisposable
{
    public DbContext Context { get; }
    private readonly IServiceScope? _scope;

    private DbContextScope(DbContext context, IServiceScope? scope)
    {
        Context = context;
        _scope = scope;
    }

    public static DbContextScope Create(IServiceProvider services, string? key = null)
    {
        var scopeFactory = services.GetService<IServiceScopeFactory>();
        if (scopeFactory != null)
        {
            var scope = scopeFactory.CreateScope();
            var ctx = (key != null
                ? scope.ServiceProvider.GetKeyedService<DbContext>(key)
                : scope.ServiceProvider.GetService<DbContext>())
                ?? throw new InvalidOperationException(
                    $"No DbContext could be resolved for type '{typeof(DbContext).FullName}'. " +
                    "Please check your dependency injection configuration. " +
                    "Recommended: use AddDbContextFactory<TContext>() + AddTransient<DbContext>(sp => sp.GetRequiredService<IDbContextFactory<TContext>>().CreateDbContext()).");
            return new DbContextScope(ctx, scope);
        }

        // Fallback: no scope factory available, resolve directly (backward compatible)
        var directCtx = (key != null
            ? services.GetKeyedService<DbContext>(key)
            : services.GetService<DbContext>())
            ?? throw new InvalidOperationException(
                $"No DbContext could be resolved for type '{typeof(DbContext).FullName}'. " +
                "Please check your dependency injection configuration.");
        return new DbContextScope(directCtx, null);
    }

    public void Dispose()
    {
        // Disposing the scope also disposes scoped services (including the DbContext).
        // If no scope, dispose the context directly.
        if (_scope != null)
            _scope.Dispose();
        else
            Context.Dispose();
    }
}
