using AutoMapper;
using DataRetrievalService.Application.DTOs;
using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;
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
            var cache = _factory.Cache();
            var item = await cache.GetAsync(id);
            if (item is not null)
            {
                return _mapper.Map<DataItemDto>(item);
            }

            var file = _factory.File();
            item = await file.GetAsync(id);
            if (item is not null)
            {
                await cache.SetAsync(item, _cacheTtl);
                return _mapper.Map<DataItemDto>(item);
            }

            var db = _factory.Database();
            item = await db.GetByIdAsync(id);
            if (item is not null)
            {
                await file.SaveAsync(item, _fileTtl);
                await cache.SetAsync(item, _cacheTtl);
                return _mapper.Map<DataItemDto>(item);
            }

            return null;
        }

        public async Task<DataItemDto> CreateAsync(CreateDataItemDto dto)
        {
            var entity = CreateDataItem(dto.Value);

            var db = _factory.Database();
            var file = _factory.File();
            var cache = _factory.Cache();

            await db.AddAsync(entity);
            await file.SaveAsync(entity, _fileTtl);
            await cache.SetAsync(entity, _cacheTtl);

            return _mapper.Map<DataItemDto>(entity);
        }

        public async Task UpdateAsync(Guid id, UpdateDataItemDto dto)
        {
            var db = _factory.Database();
            var entity = await db.GetByIdAsync(id) ?? CreateDataItem(String.Empty);
            entity.Value = dto.Value;

            await db.UpdateAsync(entity);

            var file = _factory.File();
            var cache = _factory.Cache();

            await file.SaveAsync(entity, _fileTtl);
            await cache.SetAsync(entity, _cacheTtl);
        }

        private DataItem CreateDataItem(string value) => 
            new DataItem { Id = Guid.NewGuid(), Value = value, CreatedAt = DateTime.UtcNow };

    }
}
