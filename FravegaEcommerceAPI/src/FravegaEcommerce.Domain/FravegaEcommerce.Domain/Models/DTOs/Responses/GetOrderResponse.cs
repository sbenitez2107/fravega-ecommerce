namespace FravegaEcommerceAPI.Models.DTOs.Responses
{
    public record GetOrderResponse(
        int OrderId,
        string ExternalReferenceId,
        string Channel,
        string ChannelTranslate,
        string Status,
        string StatusTranslate,
        DateTime PurchaseDate,
        decimal TotalValue,
        BuyerResponse Buyer,
        List<ProductResponse> Products,
        OrderEventResponse LastEvent
    )
    {
        public GetOrderResponse() : this(
            default, default, default, default,
            default, default, default, default,
            default, default, default)
        {
        }
    };
}
