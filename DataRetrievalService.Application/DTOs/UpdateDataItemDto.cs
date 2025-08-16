namespace DataRetrievalService.Application.DTOs;

public record UpdateDataItemDto
{
    public required string Value { get; init; }
}
