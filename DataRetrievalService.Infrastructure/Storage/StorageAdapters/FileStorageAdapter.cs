using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Infrastructure.Storage.StorageAdapters;

public sealed class FileStorageAdapter : IStorageService
{
    private readonly IFileStorageService _file;
    private readonly StorageConfiguration _config;

    public FileStorageAdapter(IFileStorageService file, StorageConfiguration config)
    {
        _file = file;
        _config = config;
    }

    public string StorageType => _config.Type;
    public int Priority => _config.Priority;
    public string StorageName => _config.Name;

    public Task<DataItem?> GetAsync(Guid id) => _file.GetAsync(id);
    
    public Task SaveAsync(DataItem item, TimeSpan ttl) => _file.SaveAsync(item, ttl);
}
