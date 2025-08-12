using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DataRetrievalService.Infrastructure.Decorators
{
    public sealed class LoggingFileStorageServiceDecorator : IFileStorageService
    {
        private readonly IFileStorageService _inner;
        private readonly ILogger<LoggingFileStorageServiceDecorator> _logger;

        public LoggingFileStorageServiceDecorator(
            IFileStorageService inner, 
            ILogger<LoggingFileStorageServiceDecorator> logger)
        { 
            _inner = inner; 
            _logger = logger; }

        public async Task<DataItem?> GetAsync(Guid id)
        {
            var sw = Stopwatch.StartNew();
            var item = await _inner.GetAsync(id);
            _logger.LogInformation("File {Result} for {Id} in {Ms} ms",
                item is null ? "MISS" : "HIT", id, sw.ElapsedMilliseconds);

            return item;
        }

        public async Task SaveAsync(DataItem item, TimeSpan ttl)
        {
            var sw = Stopwatch.StartNew();
            await _inner.SaveAsync(item, ttl);
            _logger.LogDebug("File SAVE for {Id} (ttl {Ttl} min) in {Ms} ms",
                item.Id, ttl.TotalMinutes, sw.ElapsedMilliseconds);
        }
    }
}
