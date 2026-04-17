---
title: Execution Toolkit (Paillave.Etl.ExecutionToolkit)
---

The Execution Toolkit provides helpers and utilities that complement the core ETL runtime.

## What it provides

- Execution helpers to simplify running ETL flows.
- Utilities for orchestration and integration into host applications.
- Common patterns reused by higher-level libraries.

## Example: show progress on the console

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.ExecutionToolkit;

var runner = StreamProcessRunner.Create<string>(s => s
    .CrossApply("generate", _ => new[] { 1, 2, 3 })
    .Do("work", i => Console.WriteLine(i)));

ITraceReporter traceReporter = new SimpleConsoleExecutionDisplay();

var options = new ExecutionOptions<string>
{
    TraceProcessDefinition = traceReporter.TraceProcessDefinition
};

traceReporter.Initialize(runner.GetDefinitionStructure());

await runner.ExecuteAsync("", options);
```
