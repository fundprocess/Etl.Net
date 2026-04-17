---
title: Examples
---

## Unzip files and list entries

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;

var runner = StreamProcessRunner.Create<string>(s => s
    .CrossApplyFolderFiles("list zips", "*.zip")
    .CrossApplyZipFiles("unzip", "*.csv")
    .Do("print", f => Console.WriteLine(f.Name)));

await runner.ExecuteAsync("/data/zips", new ExecutionOptions<string>());
```
