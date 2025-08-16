using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DataRetrievalService.Infrastructure.Storage.Services;

public sealed class FileStorageService : IFileStorageService
{
    private readonly string _folder;

    public FileStorageService(IConfiguration config, IOptions<FileStorageSettings> options)
    {
        var settings = options.Value;
        _folder = string.IsNullOrWhiteSpace(settings.Path)
            ? Path.Combine(AppContext.BaseDirectory, FileStorageSettings.DefaultFolderName)
            : settings.Path;

        Directory.CreateDirectory(_folder);
    }

    public async Task<DataItem?> GetAsync(Guid id)
    {
        var files = Directory.EnumerateFiles(_folder, $"{id}__*.json")
                         .OrderByDescending(f => f);

        var now = DateTime.UtcNow;

        foreach (var path in files)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var parts = name.Split("__", StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length != 2 || !long.TryParse(parts[1], out var ticks)) 
                continue;

            var exp = new DateTime(ticks, DateTimeKind.Utc);
            if (exp < now) 
            { 
                try { File.Delete(path); } catch { } 
                continue; 
            }

            var text = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<DataItem>(text);
        }
        
        return null;
    }

    public async Task SaveAsync(DataItem item, TimeSpan ttl)
    {
        var expire = DateTime.UtcNow.Add(ttl);
        var newPath = Path.Combine(_folder, $"{item.Id}__{expire.Ticks}.json");
        var json = JsonSerializer.Serialize(item);

        await File.WriteAllTextAsync(newPath, json);

        foreach (var old in Directory.EnumerateFiles(_folder, $"{item.Id}__*.json"))
        {
            if (!string.Equals(old, newPath, StringComparison.OrdinalIgnoreCase))
            {
                try { File.Delete(old); } catch { }
            }
        }
    }
}
