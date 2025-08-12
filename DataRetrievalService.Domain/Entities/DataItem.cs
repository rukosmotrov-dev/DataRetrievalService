namespace DataRetrievalService.Domain.Entities
{
    public class DataItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Value { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
