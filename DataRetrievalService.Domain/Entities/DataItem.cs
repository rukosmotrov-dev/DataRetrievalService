namespace DataRetrievalService.Domain.Entities;

public class DataItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
