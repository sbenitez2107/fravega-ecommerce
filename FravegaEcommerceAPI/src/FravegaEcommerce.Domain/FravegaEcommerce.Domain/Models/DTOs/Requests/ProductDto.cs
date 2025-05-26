namespace FravegaEcommerceAPI.Models.DTOs.Requests
{
    public record ProductDto(
        string Sku,
        string Name,
        string Description,
        decimal Price,
        int Quantity
    );
}
