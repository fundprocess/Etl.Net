---
title: Examples
---

## Read rows from an Excel sheet

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.ExcelFile;
using Paillave.Etl.FileSystem;

var runner = StreamProcessRunner.Create<string>(s => s
    .CrossApplyFolderFiles("list xlsx", "*.xlsx")
    .CrossApplyExcelSheets("sheets")
    .CrossApplyExcelRows("rows",
        map => map.UseMap(i => new
        {
            Id = i.ToNumberColumn<int>("Id"),
            Name = i.ToColumn("Name")
        }),
        (row, sheet) => new { Sheet = sheet.Name, row.Id, row.Name })
    .Do("print", r => Console.WriteLine($"{r.Sheet}: {r.Id} {r.Name}")));

await runner.ExecuteAsync("/data/excel", new ExecutionOptions<string>());
```
