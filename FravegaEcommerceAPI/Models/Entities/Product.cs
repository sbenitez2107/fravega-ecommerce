namespace FravegaEcommerceAPI.Models.Entities
{
    public class Product
    {
        public required string Sku { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
