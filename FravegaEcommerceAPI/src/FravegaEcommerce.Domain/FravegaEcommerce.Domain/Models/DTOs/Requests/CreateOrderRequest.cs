namespace FravegaEcommerceAPI.Models.DTOs.Requests
{
    public record CreateOrderRequest(
        string ExternalReferenceId,
        string Channel,
        DateTime PurchaseDate,
        decimal TotalValue,
        BuyerDto Buyer,
        List<ProductDto> Products
    );
}
