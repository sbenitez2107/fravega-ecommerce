namespace FravegaEcommerceAPI.Models.DTOs.Responses
{
    public record OrderEventResponse(
        string Id,
        string Type,
        DateTime Date,
        string? User
        //string TypeTranslate => EventTypeTranslator.Translate(Type)
    );
}
