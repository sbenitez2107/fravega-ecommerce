using FluentAssertions;
using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Models.DTOs.Requests;
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

            var eventRequest = new AddEventRequest(
                Id: "event-002",
                Type: "Cancelled",
                Date: DateTime.UtcNow,
                User: "externalUser"
            );

            var response = await clientHttp.PostAsJsonAsync($"v1/orders/{order.OrderId}/events", eventRequest);
            response.EnsureSuccessStatusCode();

            var updatedOrderCollection = await collection.Find(FilterDefinition<BsonDocument>.Empty).FirstOrDefaultAsync();
            updatedOrderCollection.Should().NotBeNull();
            updatedOrderCollection["events"].AsBsonArray.Should().NotBeEmpty();
            var events = updatedOrderCollection["events"].AsBsonArray
                .Select(e => BsonSerializer.Deserialize<Event>(e.AsBsonDocument)).ToList();
            events.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AddOrderEventTest_2_OrderNotFound_ShouldFail()
        {
            await CleanDatabase();
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = TestData.CreateAddEventRequest();
            var orderId = 1000; // Ensure this ID does not exist

            var responseOrder = await clientHttp.PostAsJsonAsync($"v1/orders/{orderId}/events", eventRequest);
            responseOrder.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await responseOrder.Content.ReadAsStringAsync();
            responseContent.Should().Contain($"Order not found");
        }

        [Fact]
        public async Task AddOrderEventTest_3_ValidationTransitionCreatedToInvoiceWrong_ShouldFail()
        {
            await CleanDatabase();
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = new AddEventRequest(
                Id: "event-002",
                Type: "Invoiced",
                Date: DateTime.UtcNow,
                User: "externalUser"
            );

            var responseOrder = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequest);
            responseOrder.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrder.Content.ReadAsStringAsync();
            responseContent.Should().Contain($"Invalid state transition from Created to {eventRequest.Type}");
        }

        [Fact]
        public async Task AddOrderEventTest_4_ValidationTransitionPaymentReceivedToReturnedWrong_ShouldFail()
        {
            await CleanDatabase();
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequestPaymentReceived = new AddEventRequest(
                Id: "event-002",
                Type: "PaymentReceived",
                Date: DateTime.UtcNow,
                User: "externalUser"
            );

            var responseOrder = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequestPaymentReceived);

            var eventRequestWrongTransition = new AddEventRequest(
                Id: "event-003",
                Type: "Returned",
                Date: DateTime.UtcNow,
                User: "externalUser"
            );

            responseOrder = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequestWrongTransition);

            responseOrder.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrder.Content.ReadAsStringAsync();
            responseContent.Should().Contain($"Invalid state transition from PaymentReceived to {eventRequestWrongTransition.Type}");
        }

        [Fact]
        public async Task AddOrderEventTest_5_ValidationEventIdRequired_ShouldFail()
        {
            await CleanDatabase();
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = new AddEventRequest(
                Id: null,
                Type: "Invoiced",
                Date: DateTime.UtcNow,
                User: "externalUser"
            );

            var responseOrder = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequest);
            responseOrder.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrder.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Id")
                .WhoseValue.Should().Contain("The Id field is required.");
        }

        [Fact]
        public async Task AddOrderEventTest_6_ValidationEventTypeRequired_ShouldFail()
        {
            await CleanDatabase();
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = new AddEventRequest(
                Id: "event-002",
                Type: null,
                Date: DateTime.UtcNow,
                User: "externalUser"
            );

            var responseOrder = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequest);
            responseOrder.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrder.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Type")
                .WhoseValue.Should().Contain("The Type field is required.");
        }

        [Fact]
        public async Task AddOrderEventTest_7_ValidationEventDateRequired_ShouldFail()
        {
            await CleanDatabase();
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = new AddEventRequest(
                Id: "event-002",
                Type: "Invoiced",
                Date: DateTime.MinValue,
                User: "externalUser"
            );

            var responseOrder = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequest);
            responseOrder.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrder.Content.ReadAsStringAsync();
            responseContent.Should().Contain("The specified condition was not met for 'Date'. Event date is required Event date must be in UTC");
        }

        [Fact]
        public async Task AddOrderEventTest_8_ValidationEventMaxLenghtID_ShouldFail()
        {
            await CleanDatabase();
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = new AddEventRequest(
                Id: new string('1', 51),
                Type: "Invoiced",
                Date: DateTime.UtcNow,
                User: "externalUser"
            );

            var responseOrderEvent = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequest);
            responseOrderEvent.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrderEvent.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Event ID max length is 50");
        }

        [Fact]
        public async Task AddOrderEventTest_9_ValidationEventMaxLenghtUser_ShouldFail()
        {
            await CleanDatabase();
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = new AddEventRequest(
                Id: "event-002",
                Type: "PaymentReceived",
                Date: DateTime.UtcNow,
                User: new string('U', 101)
            );

            var responseOrderEvent = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequest);
            responseOrderEvent.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrderEvent.Content.ReadAsStringAsync();
            responseContent.Should().Contain("User max length is 100");
        }


        [Fact]
        public async Task AddOrderEventTest_10_ValidationEventTypeWrong_ShouldFail()
        {
            await CleanDatabase();
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = new AddEventRequest(
                Id: "event-002",
                Type: "WrongType",
                Date: DateTime.UtcNow,
                User: "externalUser"
            );

            var responseOrderEvent = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequest);
            responseOrderEvent.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrderEvent.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Invalid event type");
        }

        [Fact]
        public async Task AddOrderEventTest_11_ValidationEventDateUTC_ShouldFail()
        {
            await CleanDatabase();

            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = new AddEventRequest(
                Id: "event-002",
                Type: "PaymentReceived",
                Date: DateTime.Now, // Not UTC Time
                User: "externalUser"
            );

            var responseOrderEvent = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequest);
            responseOrderEvent.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrderEvent.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Event date must be in UTC");
        }

        [Fact]
        public async Task AddOrderEventTest_12_ValidationEventDateNotFutureDate_ShouldFail()
        {
            await CleanDatabase();

            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();
            var eventRequest = new AddEventRequest(
                Id: "event-002",
                Type: "PaymentReceived",
                Date: DateTime.UtcNow.AddDays(1),
                User: "externalUser"
            );

            var responseOrderEvent = await clientHttp.PostAsJsonAsync($"v1/orders/{result.request?.OrderId}/events", eventRequest);
            responseOrderEvent.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await responseOrderEvent.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Event date cannot be in the future");
        }
    }
}
