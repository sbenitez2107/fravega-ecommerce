namespace FravegaEcommerceAPI.Models.DTOs.Responses
{
    public record BuyerResponse(
        string FirstName,
        string LastName,
        string DocumentNumber,
        string Phone
    );
}
