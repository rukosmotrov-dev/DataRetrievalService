using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Infrastructure.Storage.StorageAdapters;

public sealed class CacheStorageAdapter(ICacheService cache) : IStorageService
{
    public Task<DataItem?> GetAsync(Guid id) => cache.GetAsync(id);
    
    public Task SaveAsync(DataItem item, TimeSpan ttl) => cache.SetAsync(item, ttl);
}
