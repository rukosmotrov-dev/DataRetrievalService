using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<DataItem?> GetAsync(Guid id);
        Task SaveAsync(DataItem item, TimeSpan ttl);
    }
}
