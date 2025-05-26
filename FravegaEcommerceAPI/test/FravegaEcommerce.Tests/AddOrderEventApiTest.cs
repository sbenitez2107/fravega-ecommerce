using FluentAssertions;
using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Net.Http.Json;
using Xunit;

namespace FravegaEcommerce.Tests
{
    [Collection("ApiTest")]
    public class AddOrderEventApiTest :  TestApiRestBase
    {
        public AddOrderEventApiTest(CustomWebApplicationFactory _factory) : base(_factory)
        {
        }

        [Fact]
        public async Task AddOrderEventTest_1_Succesfull()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            // Check create order in MongoDB
            var mongoClient = factory.Services.GetRequiredService<IMongoClient>();
            var db = mongoClient.GetDatabase("FravegaDB");
            var collection = db.GetCollection<BsonDocument>("orders");

            var orderCollection = await collection.Find(FilterDefinition<BsonDocument>.Empty).FirstOrDefaultAsync();

            orderCollection.Should().NotBeNull();
            orderCollection["externalReferenceId"].AsString.Should().Be(result.request.ExternalReferenceId);

            var order = BsonSerializer.Deserialize<Order>(orderCollection);
            order.Status.Should().Be(OrderStatus.Created);
            order.TotalValue.Should().Be(result.request.TotalValue);
            order.TotalValue.Should().Be(order.Products.Sum(p => p.Price * p.Quantity));

            // Add event to order
            var eventRequest = new AddEventRequest(
                Id: "event-002",
                Type: "Cancelled",
                Date: DateTime.UtcNow,
                User: "externalUser"
            );

            var response = await clientHttp.PostAsJsonAsync($"v1/orders/{order.OrderId}/events", eventRequest);
            response.EnsureSuccessStatusCode();

            // Check event added in MongoDB
            var updatedOrderCollection = await collection.Find(FilterDefinition<BsonDocument>.Empty).FirstOrDefaultAsync();
            updatedOrderCollection.Should().NotBeNull();
            updatedOrderCollection["events"].AsBsonArray.Should().NotBeEmpty();
            var events = updatedOrderCollection["events"].AsBsonArray
                .Select(e => BsonSerializer.Deserialize<Event>(e.AsBsonDocument)).ToList();
            events.Should().NotBeEmpty();

        }
    }
}
