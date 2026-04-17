---
title: Examples
---

## Use Autofac as the service provider

```csharp
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Paillave.Etl.Core;

var builder = new ContainerBuilder();
builder.RegisterType<MyService>().As<IMyService>();
var container = builder.Build();

var services = new AutofacServiceProvider(container);

var options = new ExecutionOptions<MyConfig>
{
    Services = services
};

var runner = StreamProcessRunner.Create<MyConfig>(DefineProcess);
await runner.ExecuteAsync(new MyConfig(), options);
```
