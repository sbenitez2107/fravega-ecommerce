namespace FravegaEcommerceAPI.Models.DTOs.Requests
{
    public record OrderFilter(
        int? OrderId,
        string DocumentNumber,
        string Status,
        DateTime? CreatedOnFrom,
        DateTime? CreatedOnTo);
}
