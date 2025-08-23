using AutoMapper;
using DataRetrievalService.Application.DTOs;
using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Mapping;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using _DataRetrievalService = DataRetrievalService.Application.Services.DataRetrievalService;

namespace DataRetrievalService.Tests.Application
{
    public class DataRetrievalServiceTests
    {
        private static IMapper CreateMapper() =>
            new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

        private static IOptions<StorageSettings> Settings(int cacheMin = 10, int fileMin = 30) =>
            Options.Create(new StorageSettings
            {
                Storages = new List<StorageConfiguration>
                {
                                new StorageConfiguration { Type = "Redis Cache", Priority = 1, TtlMinutes = cacheMin, Name = "Cache Storage" },
            new StorageConfiguration { Type = "File Storage", Priority = 2, TtlMinutes = fileMin, Name = "File Storage" },
            new StorageConfiguration { Type = "MSSQL Database", Priority = 3, TtlMinutes = 0, Name = "Database Storage" }
                }
            });

        private _DataRetrievalService CreateSut(out Mock<IStorageService> cache, out Mock<IStorageService> file, out Mock<IStorageService> db, IOptions<StorageSettings>? settings = null)
        {
            cache = new Mock<IStorageService>();
            file = new Mock<IStorageService>();
            db = new Mock<IStorageService>();

            cache.Setup(s => s.StorageType).Returns("Redis Cache");
            cache.Setup(s => s.Priority).Returns(1);
            cache.Setup(s => s.StorageName).Returns("Cache Storage");
            
            file.Setup(s => s.StorageType).Returns("File Storage");
            file.Setup(s => s.Priority).Returns(2);
            file.Setup(s => s.StorageName).Returns("File Storage");
            
            db.Setup(s => s.StorageType).Returns("MSSQL Database");
            db.Setup(s => s.Priority).Returns(3);
            db.Setup(s => s.StorageName).Returns("Database Storage");

            var factory = new Mock<IStorageFactory>();
            
            factory.Setup(f => f.GetAllStorages()).Returns(new[] { cache.Object, file.Object, db.Object });

            return new _DataRetrievalService(
                factory.Object,
                CreateMapper(),
                settings ?? Settings()
            );
        }

        [Fact]
        public async Task GetAsync_returns_from_cache_first()
        {
            // Arrange
            var id = Guid.NewGuid();
            var data = "data1";
            var entity = new DataItem { Id = id, Value = data, CreatedAt = DateTime.UtcNow };
            var sut = CreateSut(out var cache, out var file, out var db);
            cache.Setup(c => c.GetAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await sut.GetAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be(data);
            cache.Verify(f => f.GetAsync(It.IsAny<Guid>()), Times.Once);
            file.Verify(f => f.GetAsync(It.IsAny<Guid>()), Times.Never);
            db.Verify(d => d.GetAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_on_cache_miss_hits_file_then_add_to_cache()
        {
            // Arrange
            var id = Guid.NewGuid();
            var data = "data2";
            var entity = new DataItem { Id = id, Value = data, CreatedAt = DateTime.UtcNow };
            var settings = Settings(cacheMin: 7, fileMin: 30);

            var sut = CreateSut(out var cache, out var file, out var db, settings);
            cache.Setup(c => c.GetAsync(id)).ReturnsAsync((DataItem?)null);
            file.Setup(f => f.GetAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await sut.GetAsync(id);

            // Assert
            result!.Value.Should().Be(data);
            cache.Verify(c => c.SaveAsync(entity, It.IsAny<TimeSpan>()), Times.Once);
            db.Verify(d => d.GetAsync(It.IsAny<Guid>()), Times.Never);
            cache.Verify(f => f.GetAsync(It.IsAny<Guid>()), Times.Once);
            file.Verify(f => f.GetAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_on_cache_and_file_miss_hits_db_and_add_to_file_and_cash()
        {
            // Arrange
            var id = Guid.NewGuid();
            var data = "data3";
            var entity = new DataItem { Id = id, Value = data, CreatedAt = DateTime.UtcNow };
            var settings = Settings(cacheMin: 10, fileMin: 25);

            var sut = CreateSut(out var cache, out var file, out var db, settings);
            cache.Setup(c => c.GetAsync(id)).ReturnsAsync((DataItem?)null);
            file.Setup(f => f.GetAsync(id)).ReturnsAsync((DataItem?)null);
            db.Setup(d => d.GetAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await sut.GetAsync(id);

            // Assert
            result!.Value.Should().Be(data);
            file.Verify(f => f.SaveAsync(entity, It.IsAny<TimeSpan>()), Times.Once);
            cache.Verify(c => c.SaveAsync(entity, It.IsAny<TimeSpan>()), Times.Once);
            db.Verify(d => d.GetAsync(It.IsAny<Guid>()), Times.Once);
            cache.Verify(f => f.GetAsync(It.IsAny<Guid>()), Times.Once);
            file.Verify(f => f.GetAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_returns_null_when_not_found_anywhere()
        {
            // Arrange
            var id = Guid.NewGuid();
            var sut = CreateSut(out var cache, out var file, out var db);

            cache.Setup(c => c.GetAsync(id)).ReturnsAsync((DataItem?)null);
            file.Setup(f => f.GetAsync(id)).ReturnsAsync((DataItem?)null);
            db.Setup(d => d.GetAsync(id)).ReturnsAsync((DataItem?)null);

            // Act
            var result = await sut.GetAsync(id);

            // Assert
            result.Should().BeNull();

            db.Verify(d => d.GetAsync(It.IsAny<Guid>()), Times.Once);
            cache.Verify(f => f.GetAsync(It.IsAny<Guid>()), Times.Once);
            file.Verify(f => f.GetAsync(It.IsAny<Guid>()), Times.Once);

            cache.Verify(c => c.SaveAsync(It.IsAny<DataItem>(), It.IsAny<TimeSpan>()), Times.Never);
            file.Verify(f => f.SaveAsync(It.IsAny<DataItem>(), It.IsAny<TimeSpan>()), Times.Never);
            db.Verify(c => c.SaveAsync(It.IsAny<DataItem>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_writes_to_db_then_file_and_cache()
        {
            // Arrange
            var settings = Settings(cacheMin: 9, fileMin: 33);
            var sut = CreateSut(out var cache, out var file, out var db, settings);
            var data = "hello";

            // Act
            var created = await sut.CreateAsync(new CreateDataItemDto { Value = data });

            // Assert
            created.Id.Should().NotBeEmpty();
            created.Value.Should().Be(data);
            db.Verify(d => d.SaveAsync(It.Is<DataItem>(d => d.Id == created.Id && d.Value == data), It.IsAny<TimeSpan>()), Times.Once);
            file.Verify(f => f.SaveAsync(It.Is<DataItem>(d => d.Id == created.Id), It.IsAny<TimeSpan>()), Times.Once);
            cache.Verify(c => c.SaveAsync(It.Is<DataItem>(d => d.Id == created.Id), It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_updates_existing_and_rewrites_file_and_cache()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new DataItem { Id = id, Value = "old", CreatedAt = DateTime.UtcNow.AddHours(-1) };
            var sut = CreateSut(out var cache, out var file, out var db);

            db.Setup(d => d.GetAsync(id)).ReturnsAsync(existing);

            // Act
            await sut.UpdateAsync(id, new UpdateDataItemDto { Value = "new" });

            // Assert
            db.Verify(d => d.SaveAsync(It.Is<DataItem>(d => d.Id == id && d.Value == "new"), It.IsAny<TimeSpan>()), Times.Once);
            file.Verify(f => f.SaveAsync(It.Is<DataItem>(d => d.Id == id && d.Value == "new"), It.IsAny<TimeSpan>()), Times.Once);
            cache.Verify(c => c.SaveAsync(It.Is<DataItem>(d => d.Id == id && d.Value == "new"), It.IsAny<TimeSpan>()), Times.Once);
        }
    }
}
