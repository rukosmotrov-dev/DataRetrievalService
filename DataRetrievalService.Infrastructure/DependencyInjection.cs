using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Infrastructure.Storage.Services;
using DataRetrievalService.Infrastructure.Decorators;
using DataRetrievalService.Infrastructure.Factories;
using DataRetrievalService.Infrastructure.Identity;
using DataRetrievalService.Infrastructure.Storage.StorageAdapters;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace DataRetrievalService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IDataRepository, DataRepository>();
        services.AddScoped<ICacheService, DistributedCacheService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IStorageFactory, StorageFactory>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<CacheStorageAdapter>();
        services.AddScoped<FileStorageAdapter>();
        services.AddScoped<DatabaseStorageAdapter>();

        services.Decorate<ICacheService, LoggingCacheServiceDecorator>();
        services.Decorate<IFileStorageService, LoggingFileStorageServiceDecorator>();
        services.Decorate<IDataRepository, LoggingDataRepositoryDecorator>();

        var shouldHandle = new PredicateBuilder()
            .Handle<TimeoutException>()
            .Handle<IOException>()
            .Handle<OperationCanceledException>();

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(200),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = shouldHandle
            })
            .AddTimeout(TimeSpan.FromSeconds(2))
            .Build();

        services.AddSingleton(pipeline);

        return services;
    }
}
