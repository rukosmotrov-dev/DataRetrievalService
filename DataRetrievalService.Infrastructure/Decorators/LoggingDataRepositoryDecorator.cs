using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DataRetrievalService.Infrastructure.Decorators;

public sealed class LoggingDataRepositoryDecorator : IDataRepository
{
    private readonly IDataRepository _inner;
    private readonly ILogger<LoggingDataRepositoryDecorator> _logger;

    public LoggingDataRepositoryDecorator(IDataRepository inner, ILogger<LoggingDataRepositoryDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<DataItem?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting item from database: {Id}", id);
        var result = await _inner.GetByIdAsync(id);
        _logger.LogInformation("Database result: {Result}", result is not null ? "Found" : "Not Found");
        return result;
    }

    public async Task AddAsync(DataItem item)
    {
        _logger.LogInformation("Adding item to database: {Id}", item.Id);
        await _inner.AddAsync(item);
        _logger.LogInformation("Item added to database successfully");
    }

    public async Task UpdateAsync(DataItem item)
    {
        _logger.LogInformation("Updating item in database: {Id}", item.Id);
        await _inner.UpdateAsync(item);
        _logger.LogInformation("Item updated in database successfully");
    }
}
