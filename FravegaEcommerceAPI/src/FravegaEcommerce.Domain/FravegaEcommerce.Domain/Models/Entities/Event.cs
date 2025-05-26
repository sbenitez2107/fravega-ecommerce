namespace FravegaEcommerceAPI.Models.Entities
{
    public class Event
    {
        public required string Id { get; set; }
        public required string Type { get; set; }
        public DateTime Date { get; set; }
        public string? User { get; set; }
    }
}
