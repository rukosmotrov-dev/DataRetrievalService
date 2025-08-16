using AutoMapper;
using DataRetrievalService.Application.DTOs;
using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;
using DataRetrievalService.Domain.Enums;
using Microsoft.Extensions.Options;

namespace DataRetrievalService.Application.Services
{
    public class DataRetrievalService : IDataRetrievalService
    {
        private readonly IStorageFactory _factory;
        private readonly IMapper _mapper;

        private readonly TimeSpan _cacheTtl;
        private readonly TimeSpan _fileTtl;

        public DataRetrievalService(
            IStorageFactory factory,
            IMapper mapper,
            IOptions<DataRetrievalSettings> settings)
        {
            _factory = factory;
            _mapper = mapper;

            var cfg = settings.Value ?? new DataRetrievalSettings();
            _cacheTtl = TimeSpan.FromMinutes(Math.Max(0, cfg.CacheTtlMinutes));
            _fileTtl = TimeSpan.FromMinutes(Math.Max(0, cfg.FileTtlMinutes));
        }

        public async Task<DataItemDto?> GetAsync(Guid id)
        {
            var cache = _factory.GetStorage(StorageType.Cache);
            var item = await cache.GetAsync(id);
            if (item is not null)
            {
                return _mapper.Map<DataItemDto>(item);
            }

            var file = _factory.GetStorage(StorageType.File);
            item = await file.GetAsync(id);
            if (item is not null)
            {
                await cache.SaveAsync(item, _cacheTtl);
                return _mapper.Map<DataItemDto>(item);
            }

            var db = _factory.GetStorage(StorageType.Database);
            item = await db.GetAsync(id);
            if (item is not null)
            {
                await file.SaveAsync(item, _fileTtl);
                await cache.SaveAsync(item, _cacheTtl);
                return _mapper.Map<DataItemDto>(item);
            }

            return null;
        }

        public async Task<DataItemDto> CreateAsync(CreateDataItemDto dto)
        {
            var entity = CreateDataItem(dto.Value);

            var db = _factory.GetStorage(StorageType.Database);
            var file = _factory.GetStorage(StorageType.File);
            var cache = _factory.GetStorage(StorageType.Cache);

            await db.SaveAsync(entity, TimeSpan.Zero);
            await file.SaveAsync(entity, _fileTtl);
            await cache.SaveAsync(entity, _cacheTtl);

            return _mapper.Map<DataItemDto>(entity);
        }

        public async Task UpdateAsync(Guid id, UpdateDataItemDto dto)
        {
            var db = _factory.GetStorage(StorageType.Database);
            var entity = await db.GetAsync(id) ?? CreateDataItem(String.Empty);
            entity.Value = dto.Value;

            await db.SaveAsync(entity, TimeSpan.Zero);
            
            var file = _factory.GetStorage(StorageType.File);
            var cache = _factory.GetStorage(StorageType.Cache);

            await file.SaveAsync(entity, _fileTtl);
            await cache.SaveAsync(entity, _cacheTtl);
        }

        private DataItem CreateDataItem(string value) => 
            new DataItem { Id = Guid.NewGuid(), Value = value, CreatedAt = DateTime.UtcNow };
    }
}
