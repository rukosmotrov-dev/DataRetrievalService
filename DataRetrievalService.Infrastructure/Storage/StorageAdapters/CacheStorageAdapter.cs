using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Infrastructure.Storage.StorageAdapters;

public sealed class CacheStorageAdapter : IStorageService
{
    private readonly ICacheService _cache;
    private readonly StorageConfiguration _config;

    public CacheStorageAdapter(ICacheService cache, StorageConfiguration config)
    {
        _cache = cache;
        _config = config;
    }

    public string StorageType => _config.Type;
    public int Priority => _config.Priority;
    public string StorageName => _config.Name;

    public Task<DataItem?> GetAsync(Guid id) => _cache.GetAsync(id);
    
    public Task SaveAsync(DataItem item, TimeSpan ttl) => _cache.SetAsync(item, ttl);
}
