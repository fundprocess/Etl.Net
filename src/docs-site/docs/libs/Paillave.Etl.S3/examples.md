---
title: Examples
---

## Download files from S3

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.S3;

var connection = new S3AdapterConnectionParameters
{
    ServiceUrl = "https://s3.example.com",
    Bucket = "my-bucket",
    AccessKeyId = "<access-key>",
    AccessKeySecret = "<secret>"
};

var provider = new S3AdapterProviderParameters
{
    SubFolder = "incoming",
    FileNamePattern = "*.csv",
    Recursive = false
};

var connectors = new FileValueConnectors()
    .Register(new S3FileValueProvider("IN", "S3 input", "S3", connection, provider));

var services = new ServiceCollection()
    .AddSingleton<IFileValueConnectors>(connectors)
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .FromConnector("read s3", "IN")
    .Do("print", f => Console.WriteLine(f.Name)));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
