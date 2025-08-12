using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Polly;
using System.Text.Json;

namespace DataRetrievalService.Infrastructure.Cache
{
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ResiliencePipeline _io;

        public DistributedCacheService(IDistributedCache cache, ResiliencePipeline io)
        {
            _cache = cache;
            _io = io;
        }

        private static string Key(Guid id) => $"data:{id}";

        public async Task<DataItem?> GetAsync(Guid id)
        {
            var bytes = await _io.ExecuteAsync(async token =>
            await _cache.GetAsync(Key(id), token));

            return bytes is null ? null : JsonSerializer.Deserialize<DataItem>(bytes);

        }

        public async Task SetAsync(DataItem item, TimeSpan ttl)
        {
            var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };

            await _io.ExecuteAsync(async token =>
                await _cache.SetAsync(Key(item.Id), JsonSerializer.SerializeToUtf8Bytes(item), opts, token));

        }
    }
}
