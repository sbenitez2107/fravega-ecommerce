namespace FravegaEcommerceAPI.Models.DTOs.Responses
{
    public record ProductResponse(
        string Sku,
        string Name,
        string Description,
        decimal Price,
        int Quantity
        //decimal TotalLine => Price * Quantity
    );
}
