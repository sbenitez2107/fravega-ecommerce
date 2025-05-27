using FluentAssertions;
using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc;
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
    public class CreateOrderApiTest : TestApiRestBase
    {
        public CreateOrderApiTest(CustomWebApplicationFactory _factory) : base(_factory)
        {
        }

        [Fact]
        public async Task CreateOrderTest_1_ShouldOK()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            // Check in MongoDB
            var mongoClient = factory.Services.GetRequiredService<IMongoClient>();
            var db = mongoClient.GetDatabase("FravegaDB");
            var collection = db.GetCollection<BsonDocument>("orders");
            var orderCollection = await collection.Find(FilterDefinition<BsonDocument>.Empty).FirstOrDefaultAsync();

            var order = BsonSerializer.Deserialize<Order>(orderCollection);
            order.Status.Should().Be(OrderStatus.Created);
            order.TotalValue.Should().Be(result.request.TotalValue);
            order.TotalValue.Should().Be(order.Products.Sum(p => p.Price * p.Quantity));
        }

        [Fact]
        public async Task CreateOrderTest_2_ExternalIdDuplicated_ShouldFail()
        {
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var requestExternalDuplicated = TestData.CreateOrderTestRequest();

            var responseExternalDuplicated = await clientHttp.PostAsJsonAsync("v1/orders", requestExternalDuplicated);
            responseExternalDuplicated.StatusCode.Should().Be(HttpStatusCode.Conflict);
            var responseContent = await responseExternalDuplicated.Content.ReadAsStringAsync();
            responseContent.Should().Contain($"ExternalReferenceId {requestExternalDuplicated?.ExternalReferenceId} must be unique per channel");

            var mongoClient = factory.Services.GetRequiredService<IMongoClient>();
            var db = mongoClient.GetDatabase("FravegaDB");
            var count = await db.GetCollection<BsonDocument>("orders")
                .CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);

            count.Should().Be(1);
        }

        [Fact]
        public async Task CreateOrderTest_3_TotalValueNotEqualToProductSum_ShouldFail()
        {
            await CleanDatabase();
            var requestTotalValueWrong = TestData.CreateOrderTestRequest();
            requestTotalValueWrong.TotalValue = 1000; // Set a wrong total value
            var sumProductsValue = requestTotalValueWrong.Products.Sum(p => p.Price * p.Quantity);

            var responseExternalTotalValueWrong = await clientHttp.PostAsJsonAsync("v1/orders", requestTotalValueWrong);
            responseExternalTotalValueWrong.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseExternalTotalValueWrong.Content.ReadAsStringAsync();
            responseContent.Should().Contain($"TotalValue {requestTotalValueWrong.TotalValue} doesn't match products sum {sumProductsValue}");
        }

        [Fact]
        public async Task CreateOrderTest_4_ValidationExternalReferenceIdRequired_ShouldFail()
        {
            await CleanDatabase();

            var requestExternalReferenceIdRequired = TestData.CreateOrderTestRequest();
            requestExternalReferenceIdRequired.ExternalReferenceId = null; // Set ExternalReferenceId to null

            var responseExternalReferenceIdRequired = await clientHttp.PostAsJsonAsync("v1/orders", requestExternalReferenceIdRequired);
            responseExternalReferenceIdRequired.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseExternalReferenceIdRequired.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("ExternalReferenceId")
                .WhoseValue.Should().Contain("The ExternalReferenceId field is required.");
        }

        [Fact]
        public async Task CreateOrderTest_5_ValidationExternalReferenceIdMaxLenght_ShouldFail()
        {
            await CleanDatabase();

            var requestExternalReferenceId = TestData.CreateOrderTestRequest();
            requestExternalReferenceId.ExternalReferenceId = new string('A', 256); // Set ExternalReferenceId to null

            var responseExternalReferenceId = await clientHttp.PostAsJsonAsync("v1/orders", requestExternalReferenceId);
            responseExternalReferenceId.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseExternalReferenceId.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("ExternalReferenceId max length is 255");
        }

        [Fact]
        public async Task CreateOrderTest_6_ValidationChannelRequired_ShouldFail()
        {
            await CleanDatabase();

            var requestChannel = TestData.CreateOrderTestRequest();
            requestChannel.Channel = null; // Set Channel to null

            var responseChannel = await clientHttp.PostAsJsonAsync("v1/orders", requestChannel);
            responseChannel.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseChannel.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Channel")
                .WhoseValue.Should().Contain("The Channel field is required");
        }

        [Fact]
        public async Task CreateOrderTest_7_ValidationChannelNotValid_ShouldFail()
        {
            await CleanDatabase();

            var requestChannel = TestData.CreateOrderTestRequest();
            requestChannel.Channel = "OnSite";

            var responseChannel = await clientHttp.PostAsJsonAsync("v1/orders", requestChannel);
            responseChannel.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseChannel.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("Invalid channel value");
        }

        [Fact]
        public async Task CreateOrderTest_8_ValidationPurchaseDateRequired_ShouldFail()
        {
            await CleanDatabase();

            var requestPurchaseDate = TestData.CreateOrderTestRequest();
            requestPurchaseDate.PurchaseDate = default; // Set PurchaseDate to default (null)

            var responsePurchaseDate = await clientHttp.PostAsJsonAsync("v1/orders", requestPurchaseDate);
            responsePurchaseDate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responsePurchaseDate.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("PurchaseDate is required");
        }

        [Fact]
        public async Task CreateOrderTest_9_ValidationPurchaseDateUTC_ShouldFail()
        {
            await CleanDatabase();

            var requestPurchaseDate = TestData.CreateOrderTestRequest();
            requestPurchaseDate.PurchaseDate = DateTime.Now; 

            var responsePurchaseDate = await clientHttp.PostAsJsonAsync("v1/orders", requestPurchaseDate);
            responsePurchaseDate.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responsePurchaseDate.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("PurchaseDate must be in UTC");
        }

        [Fact]
        public async Task CreateOrderTest_10_ValidationTotalValueGreaterThan0_ShouldFail()
        {
            await CleanDatabase();

            var requestTotalValue = TestData.CreateOrderTestRequest();
            requestTotalValue.TotalValue = 0; // Set TotalValue to 0

            var responseTotalValue = await clientHttp.PostAsJsonAsync("v1/orders", requestTotalValue);
            responseTotalValue.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseTotalValue.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("TotalValue must be greater than 0");
        }
    }
}
