namespace DataRetrievalService.Application.Options;

public sealed class FileStorageSettings
{
    public const string DefaultFolderName = "StorageFiles";
    public string Path { get; set; } = string.Empty;
    public int CleanupIntervalMinutes { get; set; } = 31;
}
