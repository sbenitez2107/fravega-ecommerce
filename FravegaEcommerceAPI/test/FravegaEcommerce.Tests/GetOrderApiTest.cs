using FluentAssertions;
using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Models.DTOs.Responses;
using FravegaEcommerceAPI.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FravegaEcommerce.Tests
{
    [Collection("ApiTest")]
    public class GetOrderApiTest : TestApiRestBase
    {
        public GetOrderApiTest(CustomWebApplicationFactory _factory) : base(_factory)
        {
        }

        [Fact]
        public async Task GetOrderTest_1_Succesfull()
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

            var response = await clientHttp.GetAsync($"v1/orders/{order.OrderId}");
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<GetOrderResponse>();
            orderResponse?.Should().NotBeNull();
            orderResponse?.OrderId.Should().Be(order.OrderId);
        }

        [Fact]
        public async Task GetOrderTest_2_OrderNotFound_ShouldFail()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var response = await clientHttp.GetAsync($"v1/orders/10");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var orderResponse = await response.Content.ReadAsStringAsync();
            orderResponse?.Should().NotBeNull();
            orderResponse?.Should().Contain($"Order not found");
        }
    }
}
