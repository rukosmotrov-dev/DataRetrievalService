namespace DataRetrievalService.Api.Contracts.Data;

public sealed class DataItemResponse
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
