namespace DataRetrievalService.Application.DTOs
{
    public class DataItemDto
    {
        public Guid Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
