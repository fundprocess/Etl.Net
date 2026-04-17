---
title: Examples
---

## Read email attachments

```csharp
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.Mail;

var connection = new MailAdapterConnectionParameters
{
    Server = "imap.example.com",
    Login = "user",
    Password = "secret",
    Ssl = true
};

var provider = new MailAdapterProviderParameters
{
    Folder = "INBOX",
    AttachmentNamePattern = "*.pdf",
    OnlyNotRead = true
};

var connectors = new FileValueConnectors()
    .Register(new MailFileValueProvider("IN", "Mail", "Mail", connection, provider));

var services = new ServiceCollection()
    .AddSingleton<IFileValueConnectors>(connectors)
    .BuildServiceProvider();

var runner = StreamProcessRunner.Create<string>(s => s
    .FromConnector("read attachments", "IN")
    .Do("print", f => Console.WriteLine(f.Name)));

await runner.ExecuteAsync("", new ExecutionOptions<string> { Services = services });
```
