using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Infrastructure.Storage.StorageAdapters;

public sealed class DatabaseStorageAdapter : IStorageService
{
    private readonly IDataRepository _repo;
    private readonly StorageConfiguration _config;

    public DatabaseStorageAdapter(IDataRepository repo, StorageConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public string StorageType => _config.Type;
    public int Priority => _config.Priority;
    public string StorageName => _config.Name;

    public Task<DataItem?> GetAsync(Guid id) => _repo.GetByIdAsync(id);
    
    public async Task SaveAsync(DataItem item, TimeSpan ttl)
    {
        if (await _repo.GetByIdAsync(item.Id) is null)
            await _repo.AddAsync(item);
        else
            await _repo.UpdateAsync(item);
    }
}
