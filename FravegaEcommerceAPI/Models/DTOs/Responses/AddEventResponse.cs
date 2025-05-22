using FravegaEcommerceAPI.Enums;

namespace FravegaEcommerceAPI.Models.DTOs.Responses
{
    public record AddEventResponse(
        int OrderId,
        OrderStatus PreviousStatus,
        OrderStatus NewStatus,
        DateTime UpdatedOn
    );
}
