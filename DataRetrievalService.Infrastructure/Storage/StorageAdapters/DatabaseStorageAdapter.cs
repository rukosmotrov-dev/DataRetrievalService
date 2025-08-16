using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Infrastructure.Storage.StorageAdapters
{
    public class DatabaseStorageAdapter : IStorageService
    {
        private readonly IDataRepository _dataRepository;

        public DatabaseStorageAdapter(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public async Task<DataItem?> GetAsync(Guid id)
        {
            return await _dataRepository.GetByIdAsync(id);
        }

        public async Task SaveAsync(DataItem item, TimeSpan ttl)
        {
            var existing = await _dataRepository.GetByIdAsync(item.Id);
            if (existing != null)
            {
                await _dataRepository.UpdateAsync(item);
            }
            else
            {
                await _dataRepository.AddAsync(item);
            }
        }
    }
}
