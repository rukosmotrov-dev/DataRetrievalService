namespace DataRetrievalService.Application.Options;

public class StorageConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Priority { get; set; } = 0;
    public int TtlMinutes { get; set; } = 0;
    public Dictionary<string, string> ConnectionSettings { get; set; } = new();
}

public class StorageSettings
{
    public List<StorageConfiguration> Storages { get; set; } = new();
}
