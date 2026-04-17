---
title: Examples
---

## Parse a Bloomberg file

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Bloomberg;
using Paillave.Etl.Core.Mapping;

var runner = StreamProcessRunner.Create<string>(s => s
    .CrossApplyFolderFiles("list files", "*.bbg")
    .CrossApplyBloombergFile("parse", i => new
    {
        LastPrice = i.ToNumberColumn<decimal>("PX_LAST", "."),
        Currency = i.ToColumn("CRNCY")
    })
    .Do("print", r => Console.WriteLine($"{r.Ticker}: {r.Values.LastPrice} {r.Values.Currency}")));

await runner.ExecuteAsync("/data/bloomberg", new ExecutionOptions<string>());
```
