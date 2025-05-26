using FravegaEcommerceAPI.Enums;

namespace FravegaEcommerceAPI.Models.DTOs.Responses
{
    public record AddEventResponse(
        long OrderId,
        string PreviousStatus,
        string NewStatus,
        DateTime UpdatedOn
    );
}
