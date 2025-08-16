using DataRetrievalService.Domain.Entities;
using DataRetrievalService.Infrastructure.Storage.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Polly;


namespace DataRetrievalService.Tests.Infrastructure;

public class DistributedCacheServiceTests
{
    private static IDistributedCache NewMemoryCache()
        => new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    private static DistributedCacheService NewSut(IDistributedCache? cache = null)
    {
        var noOp = new ResiliencePipelineBuilder().Build();
        return new DistributedCacheService(cache ?? NewMemoryCache(), noOp);
    }

    [Fact]
    public async Task Set_then_Get_roundtrips_entity()
    {
        // Arrange
        var sut = NewSut();
        var item = new DataItem { Id = Guid.NewGuid(), Value = "cached", CreatedAt = DateTime.UtcNow };

        // Act
        await sut.SetAsync(item, TimeSpan.FromMinutes(1));
        var read = await sut.GetAsync(item.Id);

        // Assert
        read.Should().NotBeNull();
        read!.Id.Should().Be(item.Id);
        read.Value.Should().Be("cached");
    }

    [Fact]
    public async Task Get_returns_null_when_key_absent()
    {
        // Arrange
        var sut = NewSut();
        var id = Guid.NewGuid();

        // Act
        var read = await sut.GetAsync(id);

        // Assert
        read.Should().BeNull();
    }

    [Fact]
    public async Task Set_with_short_ttl_expires_and_then_returns_null()
    {
        // Arrange
        var sut = NewSut();
        var item = new DataItem { Id = Guid.NewGuid(), Value = "temp", CreatedAt = DateTime.UtcNow };
        var ttl = TimeSpan.FromMilliseconds(200);

        // Act
        await sut.SetAsync(item, ttl);
        var before = await sut.GetAsync(item.Id);

        // wait past TTL (with a small buffer for timer granularity)
        await Task.Delay(ttl + TimeSpan.FromMilliseconds(200));
        var after = await sut.GetAsync(item.Id);

        // Assert
        before.Should().NotBeNull();
        after.Should().BeNull("cache entry should expire after its TTL");
    }

    [Fact]
    public async Task Set_overwrites_existing_value_for_same_id()
    {
        // Arrange
        var sut = NewSut();
        var id = Guid.NewGuid();
        var item1 = new DataItem { Id = id, Value = "v1", CreatedAt = DateTime.UtcNow };
        var item2 = new DataItem { Id = id, Value = "v2", CreatedAt = DateTime.UtcNow };

        // Act
        await sut.SetAsync(item1, TimeSpan.FromMinutes(1));
        await sut.SetAsync(item2, TimeSpan.FromMinutes(1)); // overwrite
        var read = await sut.GetAsync(id);

        // Assert
        read.Should().NotBeNull();
        read!.Value.Should().Be("v2");
    }

    [Fact]
    public async Task Different_ids_do_not_conflict()
    {
        // Arrange
        var sut = NewSut();
        var a = new DataItem { Id = Guid.NewGuid(), Value = "A", CreatedAt = DateTime.UtcNow };
        var b = new DataItem { Id = Guid.NewGuid(), Value = "B", CreatedAt = DateTime.UtcNow };

        // Act
        await sut.SetAsync(a, TimeSpan.FromMinutes(1));
        await sut.SetAsync(b, TimeSpan.FromMinutes(1));

        var readA = await sut.GetAsync(a.Id);
        var readB = await sut.GetAsync(b.Id);

        // Assert
        readA!.Value.Should().Be("A");
        readB!.Value.Should().Be("B");
    }
}
