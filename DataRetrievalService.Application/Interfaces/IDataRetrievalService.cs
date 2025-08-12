using DataRetrievalService.Application.DTOs;

namespace DataRetrievalService.Application.Interfaces
{
    public interface IDataRetrievalService
    {
        Task<DataItemDto?> GetAsync(Guid id);
        Task<DataItemDto> CreateAsync(CreateDataItemDto dto);
        Task UpdateAsync(Guid id, UpdateDataItemDto dto);
    }
}
