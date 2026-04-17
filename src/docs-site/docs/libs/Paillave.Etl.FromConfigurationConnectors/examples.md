---
title: Examples
---

## Build connectors from JSON configuration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.FromConfigurationConnectors;
using Paillave.Etl.Sftp;

var jsonConfig = File.ReadAllText("connectors.json");

var parser = new ConfigurationFileValueConnectorParser(
    new SftpProviderProcessorAdapter());

var connectors = parser.GetConnectors(jsonConfig, secret => Environment.GetEnvironmentVariable(secret) ?? secret);

var services = new ServiceCollection()
    .AddSingleton<IFileValueConnectors>(connectors)
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .FromConnector("read", "IN")
    .Do("print", f => Console.WriteLine(f.Name)));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
