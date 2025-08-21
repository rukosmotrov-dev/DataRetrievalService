using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Application.Interfaces;

public interface IStorageService : IStorageMetadata
{
    string StorageName { get; }
    Task<DataItem?> GetAsync(Guid id);
    Task SaveAsync(DataItem item, TimeSpan ttl);
}
