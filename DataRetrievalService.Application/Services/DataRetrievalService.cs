using AutoMapper;
using DataRetrievalService.Application.DTOs;
using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;
using Microsoft.Extensions.Options;

namespace DataRetrievalService.Application.Services;

public sealed class DataRetrievalService : IDataRetrievalService
{
    private readonly IMapper _mapper;
    private readonly IStorageFactory _storageFactory;
    private readonly StorageSettings _storageSettings;

    public DataRetrievalService(
        IStorageFactory storageFactory,
        IMapper mapper,
        IOptions<StorageSettings> storageSettings)
    {
        _storageFactory = storageFactory;
        _mapper = mapper;
        _storageSettings = storageSettings.Value ?? new StorageSettings();
    }

    public async Task<DataItemDto?> GetAsync(Guid id)
    {
        var storages = _storageFactory.GetAllStorages()
            .OrderBy(s => s.Priority)
            .ToList();
        
        foreach (var storage in storages)
        {
            var item = await storage.GetAsync(id);
            if (item is not null)
            {
                await PopulateStoragesByPriority(item, storage.Priority);
                return _mapper.Map<DataItemDto>(item);
            }
        }

        return null;
    }

    public async Task<DataItemDto> CreateAsync(CreateDataItemDto dto)
    {
        var dataItem = CreateDataItem(dto.Value);
        
        var storages = _storageFactory.GetAllStorages()
            .OrderBy(s => s.Priority);

        await SaveDataItemInStoragesAsync(storages, dataItem);

        return _mapper.Map<DataItemDto>(dataItem);
    }

    public async Task UpdateAsync(Guid id, UpdateDataItemDto dto)
    {
        var existingDataItem = await GetAsync(id);
        if (existingDataItem == null)
            throw new InvalidOperationException($"Record with ID - {id} not found.");

        var dataItemToUpdate = new DataItem
        {
            Id = id,
            Value = dto.Value,
            CreatedAt = existingDataItem.CreatedAt
        };

        var storages = _storageFactory.GetAllStorages()
            .OrderBy(s => s.Priority);

        await SaveDataItemInStoragesAsync(storages, dataItemToUpdate);
    }

    private async Task PopulateStoragesByPriority(DataItem dataItem, int currentPriority)
    {
        var higherPriorityStorages = _storageFactory.GetAllStorages()
            .Where(s => s.Priority < currentPriority);

        await SaveDataItemInStoragesAsync(higherPriorityStorages, dataItem);
    }

    private async Task SaveDataItemInStoragesAsync(IEnumerable<IStorageService> storages, DataItem item)
    {
        foreach (var storage in storages)
        {
            var config = GetStorageConfiguration(storage.StorageType);
            var ttl = TimeSpan.FromMinutes(config?.TtlMinutes ?? 0);
            await storage.SaveAsync(item, ttl);
        }
    }

    private StorageConfiguration? GetStorageConfiguration(string storageType)
    {
        return _storageSettings.Storages.FirstOrDefault(s => s.Type == storageType);
    }

    private static DataItem CreateDataItem(string value) => new()
    {
        Id = Guid.NewGuid(),
        Value = value,
        CreatedAt = DateTime.UtcNow
    };
}
