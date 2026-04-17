---
title: Examples
---

## Extract content from PDF files

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Pdf;

var runner = StreamProcessRunner.Create<string>(s => s
    .CrossApplyFolderFiles("list pdf", "*.pdf")
    .CrossApplyPdfContent("extract", args => args)
    .Do("print", c => Console.WriteLine($"Page {c.PageNumber}")));

await runner.ExecuteAsync("/data/pdf", new ExecutionOptions<string>());
```
