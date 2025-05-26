using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Models.Entities;

namespace FravegaEcommerceAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetOrderById(int orderId);
        Task<Order> CreateOrder(Order order);        
        Task<Order> AddEvent(int orderId, Event orderEvent);     
        Task<List<Order>> FindOrdersByFilter(OrderFilter filters);
        Task<bool> ExistsExternalReference(string externalReferenceId, string channel);
    }
}
