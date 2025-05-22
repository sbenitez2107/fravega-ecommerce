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
            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.PurchaseDate.ToUniversalTime()));

            CreateMap<BuyerDto, Buyer>();
            CreateMap<ProductDto, Product>();
            CreateMap<Event, OrderEventResponse>();
            CreateMap<Order, GetOrderResponse>()
                .ForMember(dest => dest.ChannelTranslate, opt => opt.MapFrom(src => ChannelTranslator.Translate(src.Channel)))
                .ForMember(dest => dest.StatusTranslate, opt => opt.MapFrom(src => StatusTranslator.Translate(src.Status)));

            CreateMap<Order, OrderSearchResult>()
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => src.Events.Last().Date));
        }
    }
}
