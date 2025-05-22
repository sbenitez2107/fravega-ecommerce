namespace FravegaEcommerceAPI.Models.DTOs.Responses
{
    public record CreateOrderResponse(
        int OrderId,
        string Status,
        DateTime UpdatedOn
    );
}
