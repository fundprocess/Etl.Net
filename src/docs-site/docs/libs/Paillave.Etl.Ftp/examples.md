---
title: Examples
---

## Download files from FTP

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.Ftp;

var connection = new FtpAdapterConnectionParameters
{
    Server = "ftp.example.com",
    Login = "user",
    Password = "secret",
    RootFolder = "/"
};

var provider = new FtpAdapterProviderParameters
{
    SubFolder = "incoming",
    FileNamePattern = "*.csv",
    Recursive = false
};

var connectors = new FileValueConnectors()
    .Register(new FtpFileValueProvider("IN", "FTP input", "FTP", connection, provider));

var services = new ServiceCollection()
    .AddSingleton<IFileValueConnectors>(connectors)
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .FromConnector("read ftp", "IN")
    .Do("print", f => Console.WriteLine(f.Name)));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
