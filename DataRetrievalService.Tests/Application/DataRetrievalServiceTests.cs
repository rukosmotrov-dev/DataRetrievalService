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

        private static IOptions<DataRetrievalSettings> Settings(int cacheMin = 10, int fileMin = 30) =>
            Options.Create(new DataRetrievalSettings
            {
                CacheTtlMinutes = cacheMin,
                FileTtlMinutes = fileMin
            });

        private static IDataRetrievalService CreateSut(
            out Mock<ICacheService> cache,
            out Mock<IFileStorageService> file,
            out Mock<IDataRepository> repo,
            IOptions<DataRetrievalSettings>? settings = null)
        {
            cache = new Mock<ICacheService>();
            file = new Mock<IFileStorageService>();
            repo = new Mock<IDataRepository>();

            var factory = new Mock<IStorageFactory>();
            factory.Setup(f => f.Cache()).Returns(cache.Object);
            factory.Setup(f => f.File()).Returns(file.Object);
            factory.Setup(f => f.Database()).Returns(repo.Object);

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
            var entity = new DataItem { Id = id, Value = "cached", CreatedAt = DateTime.UtcNow };
            var sut = CreateSut(out var cache, out var file, out var repo);
            cache.Setup(c => c.GetAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await sut.GetAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("cached");
            file.Verify(f => f.GetAsync(It.IsAny<Guid>()), Times.Never);
            repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_on_cache_miss_hits_file_then_primes_cache()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new DataItem { Id = id, Value = "file", CreatedAt = DateTime.UtcNow };
            var settings = Settings(cacheMin: 7, fileMin: 30);

            var sut = CreateSut(out var cache, out var file, out var repo, settings);
            cache.Setup(c => c.GetAsync(id)).ReturnsAsync((DataItem?)null);
            file.Setup(f => f.GetAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await sut.GetAsync(id);

            // Assert
            result!.Value.Should().Be("file");
            cache.Verify(c => c.SetAsync(entity, TimeSpan.FromMinutes(settings.Value.CacheTtlMinutes)), Times.Once);
            repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_on_cache_and_file_miss_hits_db_and_primes_both()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new DataItem { Id = id, Value = "db", CreatedAt = DateTime.UtcNow };
            var settings = Settings(cacheMin: 10, fileMin: 25);

            var sut = CreateSut(out var cache, out var file, out var repo, settings);
            cache.Setup(c => c.GetAsync(id)).ReturnsAsync((DataItem?)null);
            file.Setup(f => f.GetAsync(id)).ReturnsAsync((DataItem?)null);
            repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await sut.GetAsync(id);

            // Assert
            result!.Value.Should().Be("db");
            file.Verify(f => f.SaveAsync(entity, TimeSpan.FromMinutes(settings.Value.FileTtlMinutes)), Times.Once);
            cache.Verify(c => c.SetAsync(entity, TimeSpan.FromMinutes(settings.Value.CacheTtlMinutes)), Times.Once);
        }

        [Fact]
        public async Task GetAsync_returns_null_when_not_found_anywhere()
        {
            // Arrange
            var id = Guid.NewGuid();
            var sut = CreateSut(out var cache, out var file, out var repo);

            cache.Setup(c => c.GetAsync(id)).ReturnsAsync((DataItem?)null);
            file.Setup(f => f.GetAsync(id)).ReturnsAsync((DataItem?)null);
            repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((DataItem?)null);

            // Act
            var result = await sut.GetAsync(id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_writes_to_db_then_file_and_cache()
        {
            // Arrange
            var settings = Settings(cacheMin: 9, fileMin: 33);
            var sut = CreateSut(out var cache, out var file, out var repo, settings);

            // Act
            var created = await sut.CreateAsync(new CreateDataItemDto { Value = "hello" });

            // Assert
            created.Id.Should().NotBeEmpty();
            created.Value.Should().Be("hello");
            repo.Verify(r => r.AddAsync(It.Is<DataItem>(d => d.Id == created.Id && d.Value == "hello")), Times.Once);
            file.Verify(f => f.SaveAsync(It.Is<DataItem>(d => d.Id == created.Id), TimeSpan.FromMinutes(33)), Times.Once);
            cache.Verify(c => c.SetAsync(It.Is<DataItem>(d => d.Id == created.Id), TimeSpan.FromMinutes(9)), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_updates_existing_and_rewrites_file_and_cache()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new DataItem { Id = id, Value = "old", CreatedAt = DateTime.UtcNow.AddHours(-1) };
            var sut = CreateSut(out var cache, out var file, out var repo);

            repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

            // Act
            await sut.UpdateAsync(id, new UpdateDataItemDto { Value = "new" });

            // Assert
            repo.Verify(r => r.UpdateAsync(It.Is<DataItem>(d => d.Id == id && d.Value == "new")), Times.Once);
            file.Verify(f => f.SaveAsync(It.Is<DataItem>(d => d.Id == id && d.Value == "new"), It.IsAny<TimeSpan>()), Times.Once);
            cache.Verify(c => c.SetAsync(It.Is<DataItem>(d => d.Id == id && d.Value == "new"), It.IsAny<TimeSpan>()), Times.Once);
        }
    }

}
