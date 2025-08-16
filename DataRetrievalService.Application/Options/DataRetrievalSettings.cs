namespace DataRetrievalService.Application.Options;

public sealed class DataRetrievalSettings
{
    public int CacheTtlMinutes { get; set; } = 10;
    public int FileTtlMinutes { get; set; } = 30;
}
