---
title: Examples
---

## Parse XML nodes

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.XmlFile;

var runner = StreamProcessRunner.Create<string>(s => s
    .CrossApplyFolderFiles("list xml", "*.xml")
    .CrossApplyXmlFile("parse", x => x
        .AddNodeDefinition("person", "/people/person", m => new Person
        {
            Id = m.ToXPathQuery<int>("id"),
            Name = m.ToXPathQuery<string>("name")
        }))
    .XmlNodeOfType<Person>("to person", "person")
    .Do("print", p => Console.WriteLine(p.Name)));

await runner.ExecuteAsync("/data/xml", new ExecutionOptions<string>());
```
