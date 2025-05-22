using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Models.Entities;

namespace FravegaEcommerceAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetOrderById(int orderId);
        Task<Order> CreateOrder(Order order);        
        Task<Order> UpdateOrder(Order order);
        Task<Order> AddEvent(int orderId, Event @event);     
        Task<List<Order>> FindOrdersByFilter(OrderFilter filters);
        Task<Order> GetOrderByExternalReferenceIdAndChannel(string externalReferenceId, string channel);
        Task<bool> ExistsExternalReference(string externalReferenceId, string channel);
    }
}
