using AutoMapper;
using DataRetrievalService.Application.DTOs;
using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;
using DataRetrievalService.Domain.Enums;
using Microsoft.Extensions.Options;

namespace DataRetrievalService.Application.Services;

public sealed class DataRetrievalService : IDataRetrievalService
{
    private readonly IMapper _mapper;
    private readonly TimeSpan _cacheTtl;
    private readonly TimeSpan _fileTtl;
    
    private readonly IStorageService _cache;
    private readonly IStorageService _file;
    private readonly IStorageService _db;

    public DataRetrievalService(
        IStorageFactory factory,
        IMapper mapper,
        IOptions<DataRetrievalSettings> settings)
    {
        _mapper = mapper;

        var cfg = settings.Value ?? new DataRetrievalSettings();
        _cacheTtl = TimeSpan.FromMinutes(Math.Max(0, cfg.CacheTtlMinutes));
        _fileTtl = TimeSpan.FromMinutes(Math.Max(0, cfg.FileTtlMinutes));
        
        _cache = factory.GetStorage(StorageType.Cache);
        _file = factory.GetStorage(StorageType.File);
        _db = factory.GetStorage(StorageType.Database);
    }

    public async Task<DataItemDto?> GetAsync(Guid id)
    {
        var item = await _cache.GetAsync(id);
        if (item is not null)
        {
            return MapToDto(item);
        }

        item = await _file.GetAsync(id);
        if (item is not null)
        {
            await _cache.SaveAsync(item, _cacheTtl);
            return MapToDto(item);
        }

        item = await _db.GetAsync(id);
        if (item is not null)
        {
            await _file.SaveAsync(item, _fileTtl);
            await _cache.SaveAsync(item, _cacheTtl);
            return MapToDto(item);
        }

        return null;
    }

    public async Task<DataItemDto> CreateAsync(CreateDataItemDto dto)
    {
        var entity = CreateDataItem(dto.Value);

        await SaveToMultipleStorages(entity);

        return MapToDto(entity);
    }

    public async Task UpdateAsync(Guid id, UpdateDataItemDto dto)
    {
        var entity = await _db.GetAsync(id) ?? throw new Exception($"Record with ID - {id} not found.");

        entity.Value = dto.Value;

        await SaveToMultipleStorages(entity);
    }

    private async Task SaveToMultipleStorages(DataItem entity)
    {
        await _db.SaveAsync(entity, TimeSpan.Zero);
        
        await _file.SaveAsync(entity, _fileTtl);
        await _cache.SaveAsync(entity, _cacheTtl);
    }

    private DataItemDto MapToDto(DataItem item) => _mapper.Map<DataItemDto>(item);

    private static DataItem CreateDataItem(string value) => new()
    {
        Id = Guid.NewGuid(),
        Value = value,
        CreatedAt = DateTime.UtcNow
    };
}
