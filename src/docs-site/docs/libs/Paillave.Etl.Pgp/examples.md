---
title: Examples
---

## Decrypt files with PGP

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Pgp;

var connectors = new FileValueConnectors()
    .Register(new FileSystemFileValueProvider("IN", "Input", "/data/pgp", "*.pgp"))
    .Register(new PgpFileValueProcessor(
        "PGP",
        "Decrypt",
        "Pgp",
        new PgpAdapterConnectionParameters(),
        new PgpAdapterProcessorParameters
        {
            Operation = PgpOperation.Decrypt,
            PrivateKey = "<private-key>",
            Passphrase = "<passphrase>"
        }))
    .Register(new FileSystemFileValueProcessor("OUT", "Output", "/data/out"));

var services = new ServiceCollection()
    .AddSingleton<IFileValueConnectors>(connectors)
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .FromConnector("read", "IN")
    .ToConnector("decrypt", "PGP")
    .ToConnector("write", "OUT"));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
