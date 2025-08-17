using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Infrastructure.Storage.StorageAdapters;

public sealed class FileStorageAdapter(IFileStorageService file) : IStorageService
{
    public Task<DataItem?> GetAsync(Guid id) => file.GetAsync(id);
    
    public Task SaveAsync(DataItem item, TimeSpan ttl) => file.SaveAsync(item, ttl);
}
