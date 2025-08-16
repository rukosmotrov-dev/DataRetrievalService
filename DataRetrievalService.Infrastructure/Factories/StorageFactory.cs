using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Enums;
using DataRetrievalService.Infrastructure.Storage.StorageAdapters;

namespace DataRetrievalService.Infrastructure.Factories
{
    public class StorageFactory : IStorageFactory
    {
        private readonly CacheStorageAdapter _cacheAdapter;
        private readonly FileStorageAdapter _fileAdapter;
        private readonly DatabaseStorageAdapter _dbAdapter;

        public StorageFactory(
            ICacheService cache, 
            IFileStorageService file, 
            IDataRepository repo)
        {
            _cacheAdapter = new CacheStorageAdapter(cache);
            _fileAdapter = new FileStorageAdapter(file);
            _dbAdapter = new DatabaseStorageAdapter(repo);
        }

        public IStorageService GetStorage(StorageType storageType)
        {
            return storageType switch
            {
                StorageType.Cache => _cacheAdapter,
                StorageType.File => _fileAdapter,
                StorageType.Database => _dbAdapter,
                _ => throw new ArgumentException($"Unknown storage type: {storageType}", nameof(storageType))
            };
        }
    }
}
