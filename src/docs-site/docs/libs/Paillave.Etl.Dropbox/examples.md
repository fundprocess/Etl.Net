---
title: Examples
---

## Download files from Dropbox

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.Dropbox;

var connection = new DropboxAdapterConnectionParameters
{
    Token = "<token>",
    RootFolder = "/"
};

var provider = new DropboxAdapterProviderParameters
{
    SubFolder = "incoming",
    FileNamePattern = "*.csv"
};

var connectors = new FileValueConnectors()
    .Register(new DropboxFileValueProvider("IN", "Dropbox input", "Dropbox", connection, provider));

var services = new ServiceCollection()
    .AddSingleton<IFileValueConnectors>(connectors)
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .FromConnector("read dropbox", "IN")
    .Do("print", f => Console.WriteLine(f.Name)));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
