---
title: Examples
---

## Copy blobs from one folder to another

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.AzureStorageAccount;

var connection = new AzureBlobOptions
{
    DocumentContainer = "documents",
    ConnectionString = "<connection-string>"
};

var connectors = new FileValueConnectors()
    .Register(new AzureStorageAccountFileValueProvider(
        "IN",
        "Azure input",
        "Azure",
        connection,
        new AzureStorageAccountAdapterProviderParameters
        {
            SubFolder = "incoming",
            FileNamePattern = "*.csv"
        }))
    .Register(new AzureStorageAccountFileValueProcessor(
        "OUT",
        "Azure output",
        "Azure",
        connection,
        new AzureStorageAccountAdapterProcessorParameters
        {
            SubFolder = "processed",
            OverwriteIfAlreadyExists = true
        }));

var services = new ServiceCollection()
    .AddSingleton<IFileValueConnectors>(connectors)
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .FromConnector("read blobs", "IN")
    .ToConnector("write blobs", "OUT"));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
