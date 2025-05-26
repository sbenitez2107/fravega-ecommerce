namespace FravegaEcommerceAPI.Models.DTOs.Requests
{
    public record BuyerDto(
        string FirstName,
        string LastName,
        string DocumentNumber,
        string Phone
    );
}
