using FluentAssertions;
using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Models.DTOs.Responses;
using FravegaEcommerceAPI.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FravegaEcommerce.Tests
{
    [Collection("ApiTest")]
    public class SearchOrdersApiTest : TestApiRestBase
    {
        public SearchOrdersApiTest(CustomWebApplicationFactory _factory) : base(_factory)
        {
        }

        [Fact]
        public async Task SearchOrdersTest_1_SearchWithNoParameters_ShouldReturnMoreThan0()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var response = await clientHttp.GetAsync($"v1/orders/search?");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var orderResponse = await response.Content.ReadFromJsonAsync<List<GetOrderResponse>>();
            orderResponse?.Should().NotBeNull();
            orderResponse?.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task SearchOrdersTest_2_SearchNotFound_Expect0()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var response = await clientHttp.GetAsync($"v1/orders/search?orderId=10&documentNumber&status&createdOnFrom&createdOnTo");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var orderResponse = await response.Content.ReadFromJsonAsync<List<GetOrderResponse>>();
            orderResponse?.Should().NotBeNull();
            orderResponse?.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchOrdersTest_3_SearchByOrderId_Succesfull()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

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

            var response = await clientHttp.GetAsync($"v1/orders/search?orderId={order.OrderId}&documentNumber&status&createdOnFrom&createdOnTo");
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<List<GetOrderResponse>>();
            orderResponse?.Should().NotBeNull();
            orderResponse?.FirstOrDefault()?.OrderId.Should().Be(order.OrderId);
            orderResponse?.FirstOrDefault()?.ExternalReferenceId.Should().Be(order.ExternalReferenceId);
        }

        [Fact]
        public async Task SearchOrdersTest_4_SearchByDocumentNumber_Succesfull()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var response = await clientHttp.GetAsync($"v1/orders/search?documentNumber={result.request?.Buyer.DocumentNumber}");
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<List<GetOrderResponse>>();
            orderResponse?.Should().NotBeNull();
            orderResponse?.FirstOrDefault()?.OrderId.Should().Be(result.request?.OrderId);
            orderResponse?.FirstOrDefault()?.ExternalReferenceId.Should().Be(result.request?.ExternalReferenceId);
        }

        [Fact]
        public async Task SearchOrdersTest_5_SearchByStatus_Succesfull()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var response = await clientHttp.GetAsync($"v1/orders/search?status={result.request?.Status}");
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<List<GetOrderResponse>>();
            orderResponse?.Should().NotBeNull();
            orderResponse?.FirstOrDefault()?.OrderId.Should().Be(result.request?.OrderId);
            orderResponse?.FirstOrDefault()?.ExternalReferenceId.Should().Be(result.request?.ExternalReferenceId);
        }

        [Fact]
        public async Task SearchOrdersTest_6_SearchByCreatedOnFromDate_Succesfull()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var response = await clientHttp.GetAsync($"v1/orders/search?createdOnFrom={result.request?.UpdatedOn.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}");
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<List<GetOrderResponse>>();
            orderResponse?.Should().NotBeNull();
            orderResponse?.FirstOrDefault()?.OrderId.Should().Be(result.request?.OrderId);
            orderResponse?.FirstOrDefault()?.ExternalReferenceId.Should().Be(result.request?.ExternalReferenceId);
        }

        [Fact]
        public async Task SearchOrdersTest_7_SearchByCreatedOnToDate_Succesfull()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var response = await clientHttp.GetAsync($"v1/orders/search?CreatedOnTo={DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}");
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<List<GetOrderResponse>>();
            orderResponse.Should().NotBeNull();
            orderResponse?.Should().NotBeEmpty();
        }

        [Fact]
        public async Task SearchOrdersTest_8_SearchByCreatedFromAndToDate_Succesfull()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var response = await clientHttp.GetAsync($"v1/orders/search?createdOnTo={DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}&createdOnFrom={result.request?.UpdatedOn.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}");
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<List<GetOrderResponse>>();
            orderResponse.Should().NotBeNull();
            orderResponse?.Should().NotBeEmpty();
        }

        [Fact]
        public async Task SearchOrdersTest_9_SearchByCreatedFromGreaterThanToDate_Expect0()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var response = await clientHttp.GetAsync($"v1/orders/search?" +
                $"createdOnTo={result.request?.UpdatedOn.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}&createdOnFrom={DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}");
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<List<GetOrderResponse>>();
            orderResponse.Should().NotBeNull();
            orderResponse?.Should().BeEmpty();
        }
    }
}
