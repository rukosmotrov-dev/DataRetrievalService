using DataRetrievalService.Domain.Entities;
using DataRetrievalService.Infrastructure.Persistence;
using DataRetrievalService.Infrastructure.Storage.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DataRetrievalService.Tests.Infrastructure
{
    public class DataRepositoryTests
    {
        private static AppDbContext NewDb(string? name = null)
        {
            // Use a unique in-memory DB per test to avoid cross-test contamination
            var dbName = name ?? $"drs_repo_tests_{Guid.NewGuid():N}";
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            var ctx = new AppDbContext(options);
            // InMemory provider doesn't need migrations; EnsureCreated is optional here
            ctx.Database.EnsureCreated();
            return ctx;
        }

        [Fact]
        public async Task Add_and_GetById_should_roundtrip()
        {
            // Arrange
            await using var db = NewDb(nameof(Add_and_GetById_should_roundtrip));
            var repo = new DataRepository(db);

            var id = Guid.NewGuid();
            var entity = new DataItem
            {
                Id = id,
                Value = "hello",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await repo.AddAsync(entity);
            var fetched = await repo.GetByIdAsync(id);

            // Assert
            fetched.Should().NotBeNull();
            fetched!.Id.Should().Be(id);
            fetched.Value.Should().Be("hello");
        }

        [Fact]
        public async Task Update_should_overwrite_value_for_existing_entity()
        {
            // Arrange
            await using var db = NewDb(nameof(Update_should_overwrite_value_for_existing_entity));
            var repo = new DataRepository(db);

            var id = Guid.NewGuid();
            var entity = new DataItem { Id = id, Value = "old", CreatedAt = DateTime.UtcNow };
            await repo.AddAsync(entity);

            // Act
            entity.Value = "new";
            await repo.UpdateAsync(entity);
            var fetched = await repo.GetByIdAsync(id);

            // Assert
            fetched.Should().NotBeNull();
            fetched!.Value.Should().Be("new");
        }

        [Fact]
        public async Task GetById_should_return_null_when_not_found()
        {
            // Arrange
            await using var db = NewDb(nameof(GetById_should_return_null_when_not_found));
            var repo = new DataRepository(db);

            var unknownId = Guid.NewGuid();

            // Act
            var fetched = await repo.GetByIdAsync(unknownId);

            // Assert
            fetched.Should().BeNull();
        }

        [Fact]
        public async Task Add_should_not_cross_contaminate_multiple_entities()
        {
            // Arrange
            await using var db = NewDb(nameof(Add_should_not_cross_contaminate_multiple_entities));
            var repo = new DataRepository(db);

            var a = new DataItem { Id = Guid.NewGuid(), Value = "A", CreatedAt = DateTime.UtcNow };
            var b = new DataItem { Id = Guid.NewGuid(), Value = "B", CreatedAt = DateTime.UtcNow };

            // Act
            await repo.AddAsync(a);
            await repo.AddAsync(b);

            var fetchedA = await repo.GetByIdAsync(a.Id);
            var fetchedB = await repo.GetByIdAsync(b.Id);

            // Assert
            fetchedA.Should().NotBeNull();
            fetchedB.Should().NotBeNull();
            fetchedA!.Value.Should().Be("A");
            fetchedB!.Value.Should().Be("B");
            fetchedA.Id.Should().NotBe(fetchedB!.Id);
        }
    }

}
