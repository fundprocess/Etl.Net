using System;
using Polly;
using Polly.Retry;

namespace Paillave.Etl.Core;

public class ActionRunner
{
    private static ResiliencePipeline BuildPipeline(int maxAttempts) =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = maxAttempts - 1,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();

    private static ResiliencePipeline<T> BuildPipeline<T>(int maxAttempts) =>
        new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = maxAttempts - 1,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>()
            })
            .Build();

    public static void TryExecute(int attempts, Action action)
    {
        if (attempts <= 1)
        {
            action();
            return;
        }
        BuildPipeline(attempts).Execute(action);
    }

    public static T TryExecute<T>(int attempts, Func<T> action)
    {
        if (attempts <= 1)
            return action();
        return BuildPipeline<T>(attempts).Execute(action);
    }
}