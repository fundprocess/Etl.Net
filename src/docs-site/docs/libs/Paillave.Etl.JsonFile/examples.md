---
title: Examples
---

## Parse JSON files and re-serialize

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.JsonFile;

var runner = StreamProcessRunner.Create<string>(s => s
    .CrossApplyFolderFiles("list json", "*.json")
    .ParseJson<MyDto>("parse")
    .Select("transform", d => new { d.Id, d.Name })
    .SerializeToJsonFileValue("serialize")
    .WriteToFile("write", f => $"/tmp/{f.Name}"));

await runner.ExecuteAsync("/data/json", new ExecutionOptions<string>());
```
