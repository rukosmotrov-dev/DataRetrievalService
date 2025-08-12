using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DataRetrievalService.Infrastructure.Decorators
{
    public sealed class LoggingDataRepositoryDecorator : IDataRepository
    {
        private readonly IDataRepository _inner;
        private readonly ILogger<LoggingDataRepositoryDecorator> _logger;

        public LoggingDataRepositoryDecorator(
            IDataRepository inner, 
            ILogger<LoggingDataRepositoryDecorator> logger)
        { 
            _inner = inner; 
            _logger = logger; }

        public async Task<DataItem?> GetByIdAsync(Guid id)
        {
            var sw = Stopwatch.StartNew();
            var item = await _inner.GetByIdAsync(id);
            _logger.LogInformation("DB {Result} for {Id} in {Ms} ms",
                item is null ? "MISS" : "HIT", id, sw.ElapsedMilliseconds);
            return item;
        }

        public async Task AddAsync(DataItem item)
        {
            var sw = Stopwatch.StartNew();
            await _inner.AddAsync(item);
            _logger.LogInformation("DB ADD {Id} in {Ms} ms", item.Id, sw.ElapsedMilliseconds);
        }

        public async Task UpdateAsync(DataItem item)
        {
            var sw = Stopwatch.StartNew();
            await _inner.UpdateAsync(item);
            _logger.LogInformation("DB UPDATE {Id} in {Ms} ms", item.Id, sw.ElapsedMilliseconds);
        }
    }

}
