---
title: Examples
---

## Read and save with EF Core

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.EntityFrameworkCore;

var services = new ServiceCollection()
    .AddDbContextFactory<MyDbContext>(o => o.UseSqlServer("<connection-string>"))
    .AddTransient<DbContext>(sp => sp.GetRequiredService<IDbContextFactory<MyDbContext>>().CreateDbContext())
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .Select("criteria", _ => new { MinReputation = 100 })
    .EfCoreSelect("load people", (db, c) => db.Set<Person>().Where(p => p.Reputation >= c.MinReputation))
    .Select("promote", p => new Person { Email = p.Email, Reputation = p.Reputation + 1 })
    .EfCoreSave("save people", o => o.SeekOn(i => i.Email)));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
