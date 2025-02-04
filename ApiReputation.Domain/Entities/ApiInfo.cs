namespace ApiReputation.Domain.Entities
{
    public class ApiInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Category { get; set; }
        public string BaseUrl { get; set; }
        public string? Description { get; set; }
        public string? SecurityType { get; set; }
        public int? Latency { get; set; }
    }
}
