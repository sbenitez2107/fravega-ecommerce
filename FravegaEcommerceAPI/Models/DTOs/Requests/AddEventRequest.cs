namespace FravegaEcommerceAPI.Models.DTOs.Requests
{
    public record AddEventRequest(
        string Id,
        string Type,
        DateTime Date,
        string? User
    );
}
