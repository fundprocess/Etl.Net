---
title: Examples
---

## Read email attachments with Graph API

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.GraphApi;

var connection = new GraphApiAdapterConnectionParameters
{
    TenantId = "<tenant>",
    ClientId = "<client-id>",
    ClientSecret = "<client-secret>",
    UserId = "user@domain.com"
};

var provider = new GraphApiMailAdapterProviderParameters
{
    Folder = "Inbox",
    AttachmentNamePattern = "*.pdf",
    OnlyNotRead = true
};

var connectors = new FileValueConnectors()
    .Register(new GraphApiMailFileValueProvider("IN", "Mail", "GraphApi", connection, provider));

var services = new ServiceCollection()
    .AddSingleton<IFileValueConnectors>(connectors)
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .FromConnector("read attachments", "IN")
    .Do("print", f => Console.WriteLine(f.Name)));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
