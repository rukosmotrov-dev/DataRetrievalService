using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Infrastructure.Decorators;
using DataRetrievalService.Infrastructure.Factories;
using DataRetrievalService.Infrastructure.Identity;
using DataRetrievalService.Infrastructure.Storage.Services;
using DataRetrievalService.Infrastructure.Storage.StorageAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        services.AddScoped<IStorageService>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<StorageSettings>>();
            var cacheConfig = options.Value.Storages.FirstOrDefault(s => s.Type == "Cache");
            var cacheService = provider.GetRequiredService<ICacheService>();
            return new CacheStorageAdapter(cacheService, cacheConfig ?? new StorageConfiguration { Type = "Cache", Name = "Cache", Priority = 1 });
        });

        services.AddScoped<IStorageService>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<StorageSettings>>();
            var fileConfig = options.Value.Storages.FirstOrDefault(s => s.Type == "File");
            var fileService = provider.GetRequiredService<IFileStorageService>();
            return new FileStorageAdapter(fileService, fileConfig ?? new StorageConfiguration { Type = "File", Name = "File", Priority = 2 });
        });

        services.AddScoped<IStorageService>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<StorageSettings>>();
            var dbConfig = options.Value.Storages.FirstOrDefault(s => s.Type == "Database");
            var dataRepository = provider.GetRequiredService<IDataRepository>();
            return new DatabaseStorageAdapter(dataRepository, dbConfig ?? new StorageConfiguration { Type = "Database", Name = "Database", Priority = 3 });
        });

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
