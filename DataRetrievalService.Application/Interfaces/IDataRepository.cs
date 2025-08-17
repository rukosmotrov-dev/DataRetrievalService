using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Application.Interfaces;

public interface IDataRepository
{
    Task<DataItem?> GetByIdAsync(Guid id);
    Task AddAsync(DataItem item);
    Task UpdateAsync(DataItem item);
}
