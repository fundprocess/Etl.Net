---
title: Examples
---

## Upsert with EfSaveAsync

```csharp
using Microsoft.EntityFrameworkCore;
using Paillave.EntityFrameworkCoreExtension.EfSave;

await using var db = new MyDbContext();

var people = new List<Person>
{
    new Person { Email = "a@x.com", FirstName = "Ann" },
    new Person { Email = "b@x.com", FirstName = "Ben" }
};

await db.EfSaveAsync(people, p => p.Email);
```

## Bulk save

```csharp
using System.Threading;
using Paillave.EntityFrameworkCoreExtension.BulkSave;

// Insert or update by a pivot key
using var db = new MyDbContext();

db.BulkSave(people, cancellationToken: CancellationToken.None, pivotKey: p => p.Email);
```
