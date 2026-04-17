using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.EntityFrameworkCore;

internal static class ServiceProviderExtensions
{
    public static DbContextScope CreateDbContextScope(this IServiceProvider services, string? key = null)
        => DbContextScope.Create(services, key);
}