---
title: Examples
---

## Query and save with SqlServer

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.SqlServer;

var runner = StreamProcessRunner.Create<string>(s => s
    .Select("criteria", _ => new { MinReputation = 100 })
    .CrossApplySqlServerQuery("load people", o => o
        .FromQuery("select * from dbo.Person where Reputation >= @MinReputation")
        .WithMapping<Person>())
    .Select("update", p => new { p.Id, Reputation = p.Reputation + 1 })
    .ToSqlCommand("update reputation",
        "update dbo.Person set Reputation = @Reputation where Id = @Id"));

await runner.ExecuteAsync("", new ExecutionOptions<string>());
```
