using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DataRetrievalService.Infrastructure.Decorators;

public sealed class LoggingCacheServiceDecorator : ICacheService
{
    private readonly ICacheService _inner;
    private readonly ILogger<LoggingCacheServiceDecorator> _logger;

    public LoggingCacheServiceDecorator(ICacheService inner, ILogger<LoggingCacheServiceDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<DataItem?> GetAsync(Guid id)
    {
        _logger.LogInformation("Getting item from cache: {Id}", id);
        var result = await _inner.GetAsync(id);
        _logger.LogInformation("Cache result: {Result}", result is not null ? "Hit" : "Miss");
        return result;
    }

    public async Task SetAsync(DataItem item, TimeSpan ttl)
    {
        _logger.LogInformation("Setting item in cache: {Id}, TTL: {Ttl}", item.Id, ttl);
        await _inner.SetAsync(item, ttl);
        _logger.LogInformation("Item cached successfully");
    }
}
