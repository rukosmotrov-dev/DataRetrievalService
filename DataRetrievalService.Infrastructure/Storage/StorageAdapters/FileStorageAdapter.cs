using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Infrastructure.Storage.StorageAdapters
{
    public class FileStorageAdapter : IStorageService
    {
        private readonly IFileStorageService _fileStorageService;

        public FileStorageAdapter(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<DataItem?> GetAsync(Guid id)
        {
            return await _fileStorageService.GetAsync(id);
        }

        public async Task SaveAsync(DataItem item, TimeSpan ttl)
        {
            await _fileStorageService.SaveAsync(item, ttl);
        }
    }
}
