---
title: Examples
---

## Download files from SFTP

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.Sftp;

var connection = new SftpAdapterConnectionParameters
{
    Server = "sftp.example.com",
    Login = "user",
    Password = "secret",
    RootFolder = "/"
};

var provider = new SftpAdapterProviderParameters
{
    SubFolder = "incoming",
    FileNamePattern = "*.csv"
};

var connectors = new FileValueConnectors()
    .Register(new SftpFileValueProvider("IN", "Sftp input", "Sftp", connection, provider));

var services = new ServiceCollection()
    .AddSingleton<IFileValueConnectors>(connectors)
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .FromConnector("read sftp", "IN")
    .Do("print", f => Console.WriteLine(f.Name)));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
