using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Application.Interfaces
{
    public interface ICacheService
    {
        Task<DataItem?> GetAsync(Guid id);
        Task SetAsync(DataItem item, TimeSpan ttl);
    }
}
