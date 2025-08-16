namespace DataRetrievalService.Application.DTOs;

public record CreateDataItemDto
{
    public required string Value { get; init; }
}
