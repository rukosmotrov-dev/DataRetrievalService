using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Infrastructure.Storage.StorageAdapters
{
    public class CacheStorageAdapter : IStorageService
    {
        private readonly ICacheService _cacheService;

        public CacheStorageAdapter(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<DataItem?> GetAsync(Guid id)
        {
            return await _cacheService.GetAsync(id);
        }

        public async Task SaveAsync(DataItem item, TimeSpan ttl)
        {
            await _cacheService.SetAsync(item, ttl);
        }
    }
}
