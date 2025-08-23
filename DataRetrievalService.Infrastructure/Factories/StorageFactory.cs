using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataRetrievalService.Infrastructure.Factories;

public sealed class StorageFactory : IStorageFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StorageFactory> _logger;
    private readonly StorageSettings _storageSettings;
    private readonly Dictionary<string, IStorageService> _storageDictionary = new();

    public StorageFactory(
        IServiceProvider serviceProvider, 
        ILogger<StorageFactory> logger,
        IOptions<StorageSettings> storageSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _storageSettings = storageSettings.Value ?? new StorageSettings();

        CreateConfiguredStorages();
    }

    private void CreateConfiguredStorages()
    {
        var storageServices = _serviceProvider.GetServices<IStorageService>();

        if(storageServices.Count() < 1)
        {
            var errorMesage = "No storage services are configured. At least one storage service must be registered in the dependency injection container.";
            _logger.LogError(errorMesage);
            throw new InvalidOperationException(errorMesage);
        }

        foreach (var storage in storageServices)
        {
            var config = _storageSettings.Storages.FirstOrDefault(s => s.Type == storage.StorageType);
            if (config != null)
            {
                _storageDictionary[storage.StorageType] = storage;
                _logger.LogInformation($"Storage registered: {config.Name} ({storage.StorageType}) with priority {config.Priority}.");
            }
        }
    }

    public IEnumerable<IStorageService> GetAllStorages()
    {
        return _storageDictionary.Values;
    }
}
