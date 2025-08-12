namespace DataRetrievalService.Application.Options
{
    /// <summary>
    /// Settings for cache/file TTLs, bound from appsettings.json (DataRetrieval section).
    /// </summary>
    public class DataRetrievalSettings
    {
        public int CacheTtlMinutes { get; set; }
        public int FileTtlMinutes { get; set; }
    }
}
