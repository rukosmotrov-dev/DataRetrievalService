namespace DataRetrievalService.Application.Options
{
    public class FileStorageSettings
    {
        public string? Path { get; set; }
        public const string DefaultFolderName = "StorageFiles";
        public int CleanupIntervalMinutes { get; set; }
    }
}
