using AutoMapper;
using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Models.DTOs.Responses;
using FravegaEcommerceAPI.Models.Entities;
using FravegaEcommerceAPI.Translators;

namespace FravegaEcommerceAPI.Mappers
{
    public class OrderMapper : Profile
    {
        public OrderMapper()
        {
            CreateMap<Order, CreateOrderResponse>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderId))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => src.Events.Last().Date));

            CreateMap<AddEventRequest, Event>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToUniversalTime()))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User ?? "System"));

            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.PurchaseDate.ToUniversalTime()));

            CreateMap<BuyerDto, Buyer>();
            CreateMap<ProductDto, Product>();
            CreateMap<Event, OrderEventResponse>();

            CreateMap<Order, GetOrderResponse>()
                .ForCtorParam(nameof(GetOrderResponse.OrderId), opt => opt.MapFrom(src => src.OrderId))
                .ForCtorParam(nameof(GetOrderResponse.ExternalReferenceId), opt => opt.MapFrom(src => src.ExternalReferenceId))
                .ForCtorParam(nameof(GetOrderResponse.Channel), opt => opt.MapFrom(src => src.Channel))
                .ForCtorParam(nameof(GetOrderResponse.ChannelTranslate), opt => opt.MapFrom(src => ChannelTranslator.Translate(src.Channel)))
                .ForCtorParam(nameof(GetOrderResponse.Status), opt => opt.MapFrom(src => src.Status))
                .ForCtorParam(nameof(GetOrderResponse.StatusTranslate), opt => opt.MapFrom(src => StatusTranslator.Translate(src.Status)))
                .ForCtorParam(nameof(GetOrderResponse.PurchaseDate), opt => opt.MapFrom(src => src.PurchaseDate))
                .ForCtorParam(nameof(GetOrderResponse.TotalValue), opt => opt.MapFrom(src => src.TotalValue))
                .ForCtorParam(nameof(GetOrderResponse.Buyer), opt => opt.MapFrom(src => src.Buyer))
                .ForCtorParam(nameof(GetOrderResponse.Products), opt => opt.MapFrom(src => src.Products))
                .ForCtorParam(nameof(GetOrderResponse.LastEvent), opt => opt.MapFrom(src => src.Events.LastOrDefault()));

            CreateMap<Buyer, BuyerResponse>();
            CreateMap<Product, ProductResponse>();
            CreateMap<Event, OrderEventResponse>();

            CreateMap<Order, OrderSearchResult>()
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => src.Events.Last().Date));
        }
    }
}
