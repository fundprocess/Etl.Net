---
title: Examples
---

## Call HTTP endpoints and parse JSON

```csharp
using Paillave.Etl.Core;
using Paillave.Etl.Http;
using Paillave.Etl.JsonFile;

var http = new HttpFileValueProvider(
    "HTTP",
    "Http",
    "Http",
    new HttpAdapterConnectionParameters
    {
        Url = "https://api.example.com/items/{{ Id }}"
    },
    new HttpAdapterProviderParameters
    {
        Method = HttpMethodCustomEnum.Get,
        ResponseFormat = RequestFormat.Json
    });

var runner = StreamProcessRunner.Create<object>(s => s
    .CrossApply("call http", http)
    .ParseJson<ItemDto>("parse")
    .Do("print", item => Console.WriteLine(item.Id)));

await runner.ExecuteAsync(new { Id = 42 }, new ExecutionOptions<object>());
```
