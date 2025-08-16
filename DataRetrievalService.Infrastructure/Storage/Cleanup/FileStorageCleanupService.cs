using DataRetrievalService.Application.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataRetrievalService.Infrastructure.Storage.Cleanup;

public sealed class FileStorageCleanupService(
    IOptions<FileStorageSettings> options,
    ILogger<FileStorageCleanupService> logger) : BackgroundService
{
    private string ResolveFolder() =>
        string.IsNullOrWhiteSpace(options.Value.Path)
            ? Path.Combine(AppContext.BaseDirectory, FileStorageSettings.DefaultFolderName)
            : options.Value.Path;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var folder = ResolveFolder();
        Directory.CreateDirectory(folder);

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("File cleanup sweep starting in Folder: {Folder}", folder);

            try
            {
                var now = DateTime.UtcNow;
                foreach (var path in Directory.EnumerateFiles(folder, "*__*.json"))
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    var parts = name.Split("__", StringSplitOptions.RemoveEmptyEntries);
                    
                    if (parts.Length != 2 || !long.TryParse(parts[1], out var ticks)) 
                        continue;

                    if (new DateTime(ticks, DateTimeKind.Utc) <= now)
                    {
                        try { File.Delete(path); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "File cleanup sweep failed");
            }

            var minutes = Math.Clamp(options.Value.CleanupIntervalMinutes, 1, 1440);
            try 
            { 
                await Task.Delay(TimeSpan.FromMinutes(minutes), stoppingToken); 
            }
            catch (OperationCanceledException) { }
        }
    }
}
