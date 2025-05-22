using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Models.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace FravegaEcommerceAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly IMongoCollection<Counter> _counters;

        public OrderRepository(IMongoDatabase database)
        {
            _orders = database.GetCollection<Order>("orders");
            _counters = database.GetCollection<Counter>("counters");
        }

        public async Task<Order> GetOrderById(int orderId)
        {
            return await _orders.Find(o => o.OrderId == orderId).FirstOrDefaultAsync();
        }

        public async Task<Order> CreateOrder(Order order)
        {
            order.OrderId = await GetNextOrderId();
            await _orders.InsertOneAsync(order);
            return order;
        }

        private async Task<int> GetNextOrderId()
        {
            var filter = Builders<Counter>.Filter.Eq(c => c.Id, "orderId");
            var update = Builders<Counter>.Update.Inc(c => c.Sequence, 1);
            var options = new FindOneAndUpdateOptions<Counter> { ReturnDocument = ReturnDocument.After, IsUpsert = true };

            var counter = await _counters.FindOneAndUpdateAsync(filter, update, options);
            return counter.Sequence;
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            var orderUpdated = await _orders.ReplaceOneAsync(o => o.OrderId == order.OrderId, order);
            return _orders.Find(o => o.OrderId == order.OrderId).FirstOrDefault();
        }

        public async Task<Order> AddEvent(int orderId, Event @event)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.OrderId, orderId);
            var update = Builders<Order>.Update
                .Push(o => o.Events, @event)
                .Set(o => o.Status.ToString(), @event.Type)
                .Set(o => o.UpdatedOn, DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<Order> { ReturnDocument = ReturnDocument.After };
            return await _orders.FindOneAndUpdateAsync(filter, update, options);
        }

        public async Task<List<Order>> FindOrdersByFilter(OrderFilter filters)
        {
            var query = _orders.AsQueryable();

            if (filters.OrderId.HasValue)
                query = query.Where(o => o.OrderId == filters.OrderId);

            if (!string.IsNullOrEmpty(filters.DocumentNumber))
                query = query.Where(o => o.Buyer.DocumentNumber == filters.DocumentNumber);

            if (!string.IsNullOrEmpty(filters.Status))
                query = query.Where(o => o.Status == OrderStatusHelper.FromString(filters.Status));

            if (filters.CreatedOnFrom.HasValue)
                query = query.Where(o => o.PurchaseDate >= filters.CreatedOnFrom.Value.ToUniversalTime());

            if (filters.CreatedOnTo.HasValue)
                query = query.Where(o => o.PurchaseDate <= filters.CreatedOnTo.Value.ToUniversalTime());

            return await query.ToListAsync();
        }

        public async Task<Order> GetOrderByExternalReferenceIdAndChannel(string externalReferenceId, string channel)
        {
            return await _orders.Find(o => o.ExternalReferenceId == externalReferenceId && o.Channel == channel).FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsExternalReference(string externalReferenceId, string channel)
        {
            return await _orders.CountDocumentsAsync(o =>
                o.ExternalReferenceId == externalReferenceId &&
                o.Channel == channel) > 0;
        }
    }
}
