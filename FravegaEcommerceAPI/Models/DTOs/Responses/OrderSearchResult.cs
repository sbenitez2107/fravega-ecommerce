namespace FravegaEcommerceAPI.Models.DTOs.Responses
{
    public record OrderSearchResult(
        int OrderId,
        string Status,
        DateTime UpdatedOn,
        string Channel,
        string DocumentNumber
    );
}
