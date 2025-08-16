using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Infrastructure.Storage.StorageAdapters;

public sealed class DatabaseStorageAdapter(IDataRepository repo) : IStorageService
{
    public Task<DataItem?> GetAsync(Guid id) => repo.GetByIdAsync(id);
    
    public async Task SaveAsync(DataItem item, TimeSpan ttl)
    {
        if (await repo.GetByIdAsync(item.Id) is null)
            await repo.AddAsync(item);
        else
            await repo.UpdateAsync(item);
    }
}
