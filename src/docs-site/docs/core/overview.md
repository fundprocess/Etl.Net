---
title: Core ETL (Paillave.Etl)
---

This section introduces the core ETL library that the rest of the ecosystem builds on.

## What it provides

- The primary pipeline concepts and abstractions used by other libraries.
- The common configuration and execution patterns used across connectors and processors.
- Shared conventions for composing ETL flows.

## Typical usage pattern

1. Define a flow with your inputs, transforms, and outputs.
2. Configure connectors and runtime options.
3. Execute the flow and observe outputs.

## Example: build a small in-memory pipeline

```csharp
using System;
using System.Linq;
using Paillave.Etl.Core;

var runner = StreamProcessRunner.Create<string>(s => s
    .CrossApply("generate", _ => Enumerable.Range(0, 5)
        .Select(i => new { Index = i, CategoryId = i % 2 }))
    .GroupBy("group", i => i.CategoryId)
    .Select("summary", g => new { g.FirstValue.CategoryId, Count = g.Aggregation.Count })
    .Do("print", x => Console.WriteLine($"Category {x.CategoryId}: {x.Count}")));

await runner.ExecuteAsync("", new ExecutionOptions<string>());
```
