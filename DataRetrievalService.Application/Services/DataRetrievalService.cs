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
                await PopulateStoragesWithHigherPriority(item, storage.Priority);
                return _mapper.Map<DataItemDto>(item);
            }
        }

        return null;
    }

    public async Task<DataItemDto> CreateAsync(CreateDataItemDto dto)
    {
        var entity = CreateDataItem(dto.Value);
        
        var storages = _storageFactory.GetAllStorages()
            .OrderBy(s => s.Priority);

        foreach (var storage in storages)
        {
            var config = GetStorageConfiguration(storage.StorageType);
            var ttl = TimeSpan.FromMinutes(config?.TtlMinutes ?? 0);
            await storage.SaveAsync(entity, ttl);
        }

        return _mapper.Map<DataItemDto>(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateDataItemDto dto)
    {
        var item = await GetAsync(id);
        if (item == null)
            throw new Exception($"Record with ID - {id} not found.");

        var entity = new DataItem
        {
            Id = id,
            Value = dto.Value,
            CreatedAt = item.CreatedAt
        };

        var storages = _storageFactory.GetAllStorages()
            .OrderBy(s => s.Priority);

        foreach (var storage in storages)
        {
            var config = GetStorageConfiguration(storage.StorageType);
            var ttl = TimeSpan.FromMinutes(config?.TtlMinutes ?? 0);
            await storage.SaveAsync(entity, ttl);
        }
    }

    private async Task PopulateStoragesWithHigherPriority(DataItem item, int currentPriority)
    {
        var higherPriorityStorages = _storageFactory.GetAllStorages()
            .Where(s => s.Priority > currentPriority);

        foreach (var storage in higherPriorityStorages)
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
