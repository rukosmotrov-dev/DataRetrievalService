using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Enums;
using DataRetrievalService.Infrastructure.Storage.StorageAdapters;

namespace DataRetrievalService.Infrastructure.Factories;

public sealed class StorageFactory(
    ICacheService cache,
    IFileStorageService file,
    IDataRepository repo) : IStorageFactory
{
    private readonly CacheStorageAdapter _cacheAdapter = new(cache);
    private readonly FileStorageAdapter _fileAdapter = new(file);
    private readonly DatabaseStorageAdapter _dbAdapter = new(repo);

    public IStorageService GetStorage(StorageType storageType) => storageType switch
    {
        StorageType.Cache => _cacheAdapter,
        StorageType.File => _fileAdapter,
        StorageType.Database => _dbAdapter,
        _ => throw new ArgumentException($"Unknown storage type: {storageType}", nameof(storageType))
    };
}
