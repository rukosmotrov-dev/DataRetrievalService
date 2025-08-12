using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DataRetrievalService.Infrastructure.Decorators
{
    public sealed class LoggingCacheServiceDecorator : ICacheService
    {
        private readonly ICacheService _inner;
        private readonly ILogger<LoggingCacheServiceDecorator> _logger;

        public LoggingCacheServiceDecorator(
            ICacheService inner, 
            ILogger<LoggingCacheServiceDecorator> logger)
        { 
            _inner = inner; 
            _logger = logger; }

        public async Task<DataItem?> GetAsync(Guid id)
        {
            var sw = Stopwatch.StartNew();
            var item = await _inner.GetAsync(id);
            _logger.LogInformation("Cache {Result} for {Id} in {Ms} ms",
                item is null ? "MISS" : "HIT", id, sw.ElapsedMilliseconds);

            return item;
        }

        public async Task SetAsync(DataItem item, TimeSpan ttl)
        {
            var sw = Stopwatch.StartNew();
            await _inner.SetAsync(item, ttl);
            _logger.LogDebug("Cache SET for {Id} (ttl {Ttl} min) in {Ms} ms",
                item.Id, ttl.TotalMinutes, sw.ElapsedMilliseconds);
        }
    }
}
