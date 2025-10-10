﻿using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.FileSystem;
using Paillave.Etl.TextFile;
using Paillave.Etl.EntityFrameworkCore;
using BlogTutorial.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.ComponentModel.Design;
using Microsoft.Extensions.DependencyInjection;

namespace BlogTutorial;

class Program9
{
    static async Task Main(string[] args)
    {
        using var dbCtx = new SimpleTutorialDbContext(args[1]);
        dbCtx.Database.Migrate();

        var serviceCollection = new ServiceCollection().AddSingleton<DbContext, SimpleTutorialDbContext>(sp => new SimpleTutorialDbContext(args[1]));

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var resolver = new CompositeDependencyResolver()
                            .AddResolver(new SimpleDependencyResolver()
)
                            .AddResolver(new DependencyResolver(serviceProvider));

        var ctx1 = serviceProvider.GetRequiredService<DbContext>();
        var ctx = resolver.Resolve<DbContext>();


        // var resolver = new CompositeDependencyResolver()
        //                     .AddResolver(new SimpleDependencyResolver()
        //                         .Register(processContext)
        //                         .Register(etlDiagnostic))
        //                     .AddResolver(new DependencyResolver(serviceProvider));



        var processRunner = StreamProcessRunner.Create<string>(i => i.EfCoreSelect("get posts", (o, row) => o.Set<Post>()));
        processRunner.DebugNodeStream += (sender, e)
            =>
        { /* PLACE A CONDITIONAL BREAKPOINT HERE FOR DEBUG ex: e.NodeName == "parse file" */ };

        var executionOptions = new ExecutionOptions<string>
        {
            Resolver = new SimpleDependencyResolver().Register<DbContext>(dbCtx),
            TraceProcessDefinition = (ts, cs) => ts.Do("Show trace on console", t => Console.WriteLine(t.ToString())),
            // TraceProcessDefinition = DefineTraceProcess,
            // UseDetailedTraces = true // activate only if per row traces are meant to be caught
        };
        var res = await processRunner.ExecuteAsync(args[0], executionOptions);
        if (res.ErrorTraceEvent.Content is UnhandledExceptionStreamTraceContent unhandledExceptionStreamTraceContent)
        {
            Console.WriteLine($"Error occurred in node {res.ErrorTraceEvent.NodeName}(type {res.ErrorTraceEvent.NodeTypeName}): {unhandledExceptionStreamTraceContent.Message} (exception: {unhandledExceptionStreamTraceContent.Exception})");
        }
        Console.Write(res.Failed ? "Failed" : "Succeeded");
    }
    private static void DefineTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<string> contentStream)
    {
        traceStream
            .Where("keep only summary of node and errors", i => i.Content is CounterSummaryStreamTraceContent || i.Content is UnhandledExceptionStreamTraceContent)
            .Select("create log entry", i => new ExecutionLog
            {
                DateTime = i.DateTime,
                ExecutionId = i.ExecutionId,
                EventType = i.Content switch
                {
                    CounterSummaryStreamTraceContent => "EndOfNode",
                    UnhandledExceptionStreamTraceContent => "Error",
                    _ => "Unknown"
                },
                Message = i.Content switch
                {
                    CounterSummaryStreamTraceContent counterSummary => $"{i.NodeName}: {counterSummary.Counter}",
                    UnhandledExceptionStreamTraceContent unhandledException => $"{i.NodeName}({i.NodeTypeName}): [{unhandledException.Level.ToString()}] {unhandledException.Message}",
                    _ => "Unknown"
                }
            })
          .EfCoreSave("save traces");
    }
    private static void DefineProcess2(ISingleStream<string> contextStream)
    {
        contextStream
            .Select("create a criteria", i => new { Title = "pro" })
            .EfCoreSelect("get posts", (o, row) => o
                .Set<Post>()
                .Where(i => i.Title.Contains(row.Title)));
    }
    private static void DefineProcess3(ISingleStream<string> contextStream)
    {
        contextStream
            .EfCoreSelect("get posts", (o, row) => o.Set<Post>())
            .EfCoreLookup("get related authors", o => o
                .Set<Author>()
                .On(i => i.AuthorId, i => i.Id)
                .Select((l, r) => new { Post = l, Author = r }));
    }
    private static void DefineProcess4(ISingleStream<string> contextStream)
    {
        contextStream
            .EfCoreSelect("get posts", (o, row) => o.Set<Post>())
            .EfCoreLookup("get related authors", o => o
                .Query(o => o.Set<Author>().Where(a => a.Name == "sdfsdfsd"))
                .On(i => i.AuthorId, i => i.Id)
                .Select((l, r) => new { Post = l, Author = r })
                .CreateIfNotFound(p => new Author { Name = $"Name {p.AuthorId}" })
                .NoCacheFullDataset()
                .CacheSize(500));
    }
    private static void DefineProcess(ISingleStream<string> contextStream)
    {
        var rowStream = contextStream
        .CrossApplyFolderFiles("list all required files", "*.csv", true)
        .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new
        {
            Author = i.ToColumn("author"),
            Email = i.ToColumn("email"),
            TimeSpan = i.ToDateColumn("timestamp", "yyyyMMddHHmmss"),
            Category = i.ToColumn("category"),
            Link = i.ToColumn("link"),
            Post = i.ToColumn("post"),
            Title = i.ToColumn("title"),
        }).IsColumnSeparated(','))
        .SetForCorrelation("set correlation for row");

        var authorStream = rowStream
            .Distinct("remove author duplicates based on emails", i => i.Email)
            // .Select("create author instance", i => new Author { Email = i.Email, Name = i.Author })
            .EfCoreSave("save authors", o => o
                .Entity(i => new Author { Email = i.Email, Name = i.Author })
                .SeekOn(i => i.Email)
                .AlternativelySeekOn(i => i.Name));

        var categoryStream = rowStream
            .Distinct("remove category duplicates", i => i.Category)
            .Select("create category instance", i => new Category { Code = i.Category, Name = i.Category })
            .EfCoreSave("save categories", o => o.SeekOn(i => i.Code).DoNotUpdateIfExists());

        var postStream = rowStream
            .CorrelateToSingle("get related category", categoryStream, (l, r) => new { Row = l, Category = r })
            .CorrelateToSingle("get related author", authorStream, (l, r) => new { l.Row, l.Category, Author = r })
            .Select("create post instance", i => string.IsNullOrWhiteSpace(i.Row.Post)
                ? new LinkPost
                {
                    AuthorId = i.Author.Id,
                    CategoryId = i.Category.Id,
                    DateTime = i.Row.TimeSpan,
                    Title = i.Row.Title,
                    Url = new Uri(i.Row.Link)
                } as Post
                : new TextPost
                {
                    AuthorId = i.Author.Id,
                    CategoryId = i.Category.Id,
                    DateTime = i.Row.TimeSpan,
                    Title = i.Row.Title,
                    Text = i.Row.Post
                })
            .EfCoreSave("save posts", o => o.SeekOn(i => new { i.AuthorId, i.DateTime }));
    }
}


public class DependencyResolver(IServiceProvider serviceProvider) : IDependencyResolver
{
    public object Resolve(Type type)
    {
        var resolved = serviceProvider.GetRequiredService(type);
        return resolved;
    }
    public T Resolve<T>() where T : class
    {
        var resolved = serviceProvider.GetRequiredService<T>();
        return resolved;
    }
    public T Resolve<T>(string key) where T : class => serviceProvider.GetRequiredKeyedService<T>(key);
    public bool TryResolve<T>(out T resolved) where T : class
    {
        resolved = serviceProvider.GetService<T>()!;
        return resolved != null;
    }

    public bool TryResolve<T>(string key, out T resolved) where T : class
    {
        resolved = serviceProvider.GetKeyedService<T>(key)!;
        return resolved != null;
    }

    public bool TryResolve(Type type, out object resolved)
    {
        resolved = serviceProvider.GetService(type)!;
        return resolved != null;
    }

    public object Resolve(Type type, string key) => serviceProvider.GetRequiredKeyedService(type, key);

    public bool TryResolve(Type type, string key, out object resolved)
    {
        resolved = (serviceProvider.GetKeyedServices(type, key) ?? []).FirstOrDefault()!;
        return resolved != null;
    }
}
