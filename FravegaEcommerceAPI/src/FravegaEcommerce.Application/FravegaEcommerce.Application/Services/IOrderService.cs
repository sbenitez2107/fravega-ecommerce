using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Models.DTOs.Responses;

namespace FravegaEcommerceAPI.Services
{
    public interface IOrderService
    {
        Task<CreateOrderResponse> CreateOrder(CreateOrderRequest request);
        Task<AddEventResponse> AddOrderEvent(int orderId, AddEventRequest request);
        Task<GetOrderResponse> GetOrder(int orderId);
        Task<IEnumerable<GetOrderResponse>> FindOrdersByFilter(OrderFilter filters);
    }
}
