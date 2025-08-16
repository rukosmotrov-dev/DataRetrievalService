using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;
using DataRetrievalService.Infrastructure.Storage.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DataRetrievalService.Tests.Infrastructure;

public class FileStorageServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IConfiguration _cfg;

    public FileStorageServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "drs_files_" + Guid.NewGuid().ToString("N"));
        _cfg = new ConfigurationBuilder().AddInMemoryCollection().Build();
    }

    private FileStorageService CreateSvc(string? path = null)
    {
        var opts = Options.Create(new FileStorageSettings { Path = path ?? _tempDir });
        return new FileStorageService(_cfg, opts);
    }

    private static DataItem NewItem(Guid? id = null, string value = "value") =>
        new() { Id = id ?? Guid.NewGuid(), Value = value, CreatedAt = DateTime.UtcNow };

    public void Dispose()
    {
        try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); } catch { /* ignore */ }
    }

    [Fact]
    public void Ctor_creates_folder_when_missing()
    {
        // Arrange
        Directory.Exists(_tempDir).Should().BeFalse("test starts with a fresh temp path");

        // Act
        _ = CreateSvc();

        // Assert
        Directory.Exists(_tempDir).Should().BeTrue("constructor should create the target folder");
    }

    [Fact]
    public async Task Save_then_Get_returns_item_when_not_expired()
    {
        // Arrange
        var svc = CreateSvc();
        var item = NewItem(value: "hello");

        // Act
        await svc.SaveAsync(item, TimeSpan.FromMinutes(2));
        var read = await svc.GetAsync(item.Id);

        // Assert
        Directory.GetFiles(_tempDir, $"{item.Id}__*.json").Should().HaveCount(1);
        read.Should().NotBeNull();
        read!.Id.Should().Be(item.Id);
        read.Value.Should().Be("hello");
    }

    [Fact]
    public async Task Get_deletes_expired_files_and_returns_null_if_only_expired()
    {
        // Arrange
        var svc = CreateSvc();
        var id = Guid.NewGuid();
        Directory.CreateDirectory(_tempDir);

        // create an already-expired file manually
        var expiredTicks = DateTime.UtcNow.AddMinutes(-5).Ticks;
        var expiredPath = Path.Combine(_tempDir, $"{id}__{expiredTicks}.json");
        await File.WriteAllTextAsync(expiredPath, JsonSerializer.Serialize(NewItem(id, "stale")));

        // Act
        var result = await svc.GetAsync(id);

        // Assert
        result.Should().BeNull();
        File.Exists(expiredPath).Should().BeFalse("expired file should be removed during GetAsync");
    }

    [Fact]
    public async Task Save_replaces_older_files_for_same_id()
    {
        // Arrange
        var svc = CreateSvc();
        var id = Guid.NewGuid();

        // Act
        await svc.SaveAsync(NewItem(id, "v1"), TimeSpan.FromMinutes(10));
        await svc.SaveAsync(NewItem(id, "v2"), TimeSpan.FromMinutes(10)); // should delete the first file

        var files = Directory.GetFiles(_tempDir, $"{id}__*.json");
        var read = await svc.GetAsync(id);

        // Assert
        files.Length.Should().Be(1, "SaveAsync should delete previous files for the same id");
        read.Should().NotBeNull();
        read!.Value.Should().Be("v2");
    }

    [Fact]
    public async Task Get_picks_newest_nonexpired_when_multiple_exist()
    {
        // Arrange
        var svc = CreateSvc();
        var id = Guid.NewGuid();
        Directory.CreateDirectory(_tempDir);

        var olderTicks = DateTime.UtcNow.AddMinutes(2).Ticks;
        var newerTicks = DateTime.UtcNow.AddMinutes(5).Ticks;

        var olderPath = Path.Combine(_tempDir, $"{id}__{olderTicks}.json");
        var newerPath = Path.Combine(_tempDir, $"{id}__{newerTicks}.json");

        await File.WriteAllTextAsync(olderPath, JsonSerializer.Serialize(NewItem(id, "older")));
        await File.WriteAllTextAsync(newerPath, JsonSerializer.Serialize(NewItem(id, "newer")));

        // Act
        var read = await svc.GetAsync(id);

        // Assert
        read.Should().NotBeNull();
        read!.Value.Should().Be("newer", "service orders files by ticks desc and returns first non-expired");
    }

    [Fact]
    public async Task Get_returns_null_when_no_files_for_id()
    {
        // Arrange
        var svc = CreateSvc();
        var id = Guid.NewGuid();

        // Act
        var read = await svc.GetAsync(id);

        // Assert
        read.Should().BeNull();
    }
}
