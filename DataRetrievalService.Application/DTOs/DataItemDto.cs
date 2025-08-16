namespace DataRetrievalService.Application.DTOs;

public record DataItemDto
{
    public required Guid Id { get; init; }
    public required string Value { get; init; }
    public required DateTime CreatedAt { get; init; }
}
