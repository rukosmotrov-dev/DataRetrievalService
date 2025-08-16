using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DataRetrievalService.Infrastructure.Decorators;

public sealed class LoggingFileStorageServiceDecorator : IFileStorageService
{
    private readonly IFileStorageService _inner;
    private readonly ILogger<LoggingFileStorageServiceDecorator> _logger;

    public LoggingFileStorageServiceDecorator(IFileStorageService inner, ILogger<LoggingFileStorageServiceDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<DataItem?> GetAsync(Guid id)
    {
        _logger.LogInformation("Getting item from file storage: {Id}", id);
        var result = await _inner.GetAsync(id);
        _logger.LogInformation("File storage result: {Result}", result is not null ? "Found" : "Not Found");
        return result;
    }

    public async Task SaveAsync(DataItem item, TimeSpan ttl)
    {
        _logger.LogInformation("Saving item to file storage: {Id}, TTL: {Ttl}", item.Id, ttl);
        await _inner.SaveAsync(item, ttl);
        _logger.LogInformation("Item saved to file storage successfully");
    }
}
