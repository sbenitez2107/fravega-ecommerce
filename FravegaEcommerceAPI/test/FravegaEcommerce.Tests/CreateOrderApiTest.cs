using FluentAssertions;
using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Models.DTOs.Responses;
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
        public async Task CreateOrderTest_2_ExternalIdDuplicated_ShouldSuccessfull()
        {
            // Idempotency test: if the same ExternalReferenceId is sent, it should return the same order without creating a new one.
            (Order? request, HttpResponseMessage response) result = await CreateNewOrder();

            var requestExternalDuplicated = TestData.CreateOrderTestRequest();

            var responseExternalDuplicated = await clientHttp.PostAsJsonAsync("v1/orders", requestExternalDuplicated);
            responseExternalDuplicated.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await responseExternalDuplicated.Content.ReadFromJsonAsync<CreateOrderResponse>();
            responseContent?.OrderId.Should().Be(1);
            responseContent?.Status.Should().Be(OrderStatus.Created.ToString());
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
            requestExternalReferenceId.ExternalReferenceId = new string('A', 256); // Set ExternalReferenceId with 256 characters

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
                .WhoseValue.Should().Contain("The Channel field is required.");
        }

        [Fact]
        public async Task CreateOrderTest_7_ValidationChannelNotValid_ShouldFail()
        {
            await CleanDatabase();

            var requestChannel = TestData.CreateOrderTestRequest();
            requestChannel.Channel = "OnSite"; // channel not valid

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
            requestPurchaseDate.PurchaseDate = DateTime.Now; // Not UTC Time 

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

        [Fact]
        public async Task CreateOrderTest_11_ValidationBuyerIsNull_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerNull = TestData.CreateOrderTestRequest();
            requestBuyerNull.Buyer = null; // Set Buyer to null

            var responseBuyerNull = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerNull);
            responseBuyerNull.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerNull.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Buyer")
                .WhoseValue.Should().Contain("The Buyer field is required.");
        }

        [Fact]
        public async Task CreateOrderTest_12_ValidationBuyerRequiredFirstname_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerRequired = TestData.CreateOrderTestRequest();
            requestBuyerRequired.Buyer.FirstName = null; // Set Buyer firstname to null

            var responseBuyerRequired = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerRequired);
            responseBuyerRequired.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerRequired.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Buyer.FirstName")
                .WhoseValue.Should().Contain("The FirstName field is required.");
        }

        [Fact]
        public async Task CreateOrderTest_13_ValidationBuyerRequiredLastName_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerRequired = TestData.CreateOrderTestRequest();
            requestBuyerRequired.Buyer.LastName = null; // Set Buyer lastName to null

            var responseBuyerRequired = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerRequired);
            responseBuyerRequired.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerRequired.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Buyer.LastName")
                .WhoseValue.Should().Contain("The LastName field is required.");
        }

        [Fact]
        public async Task CreateOrderTest_14_ValidationBuyerRequiredDocumentNumber_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerRequired = TestData.CreateOrderTestRequest();
            requestBuyerRequired.Buyer.DocumentNumber = null; // Set Buyer DocumentNumber to null

            var responseBuyerRequired = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerRequired);
            responseBuyerRequired.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerRequired.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Buyer.DocumentNumber")
                .WhoseValue.Should().Contain("The DocumentNumber field is required.");
        }

        [Fact]
        public async Task CreateOrderTest_15_ValidationBuyerRequiredPhone_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerRequired = TestData.CreateOrderTestRequest();
            requestBuyerRequired.Buyer.Phone = null; // Set Buyer phone to null

            var responseBuyerRequired = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerRequired);
            responseBuyerRequired.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerRequired.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Buyer.Phone")
                .WhoseValue.Should().Contain("The Phone field is required.");
        }

        [Fact]
        public async Task CreateOrderTest_16_ValidationBuyerFirstNameMaxLenght_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerFirstName = TestData.CreateOrderTestRequest();
            requestBuyerFirstName.Buyer.FirstName = new string('A', 101); // Set Buyer FirstName with 101 characters

            var responseBuyerFirstName = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerFirstName);
            responseBuyerFirstName.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerFirstName.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("FirstName max length is 100");
        }

        [Fact]
        public async Task CreateOrderTest_17_ValidationBuyerLastNameMaxLenght_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerLastName = TestData.CreateOrderTestRequest();
            requestBuyerLastName.Buyer.LastName = new string('A', 101); // Set Buyer LastName with 101 characters

            var responseBuyerLastName = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerLastName);
            responseBuyerLastName.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerLastName.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("LastName max length is 100");
        }

        [Fact]
        public async Task CreateOrderTest_18_ValidationBuyerDocumentNumberMaxLenght_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerDocumentNumber = TestData.CreateOrderTestRequest();
            requestBuyerDocumentNumber.Buyer.DocumentNumber = new string('1', 21); // Set Buyer DocumentNumber with 21 characters

            var responseBuyerDocumentNumber = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerDocumentNumber);
            responseBuyerDocumentNumber.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerDocumentNumber.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("DocumentNumber max length is 20");
        }

        [Fact]
        public async Task CreateOrderTest_19_ValidationBuyerDocumentNumberMinLenght_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerDocumentNumber = TestData.CreateOrderTestRequest();
            requestBuyerDocumentNumber.Buyer.DocumentNumber = new string('1', 4); // Set Buyer DocumentNumber with 4 characters

            var responseBuyerDocumentNumber = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerDocumentNumber);
            responseBuyerDocumentNumber.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerDocumentNumber.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("DocumentNumber min length is 5");
        }

        [Fact]
        public async Task CreateOrderTest_20_ValidationBuyerPhoneInvalidFormat_ShouldFail()
        {
            await CleanDatabase();

            var requestBuyerPhone = TestData.CreateOrderTestRequest();
            requestBuyerPhone.Buyer.Phone = "128EEE8888888E";

            var responseBuyerPhone = await clientHttp.PostAsJsonAsync("v1/orders", requestBuyerPhone);
            responseBuyerPhone.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseBuyerPhone.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("Invalid phone number format");
        }

        [Fact]
        public async Task CreateOrderTest_21_ValidationProductRequireSKU_ShouldFail()
        {
            await CleanDatabase();

            var requestProductSKU = TestData.CreateOrderTestRequest();
            requestProductSKU.Products[0].Sku = null;

            var responseProductSKU = await clientHttp.PostAsJsonAsync("v1/orders", requestProductSKU);
            responseProductSKU.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseProductSKU.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Products[0].Sku")
                .WhoseValue.Should().Contain("The Sku field is required.");
        }

        [Fact]
        public async Task CreateOrderTest_22_ValidationProductSKUMaxLenght_ShouldFail()
        {
            await CleanDatabase();

            var requestProductSKU = TestData.CreateOrderTestRequest();
            requestProductSKU.Products[0].Sku = new string('A', 51); // Set Product SKU with 51 characters

            var responseProductSKU = await clientHttp.PostAsJsonAsync("v1/orders", requestProductSKU);
            responseProductSKU.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseProductSKU.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("SKU max length is 50");
        }

        [Fact]
        public async Task CreateOrderTest_23_ValidationProductRequireName_ShouldFail()
        {
            await CleanDatabase();

            var requestProductName = TestData.CreateOrderTestRequest();
            requestProductName.Products[0].Name = null;

            var responseProductName = await clientHttp.PostAsJsonAsync("v1/orders", requestProductName);
            responseProductName.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseProductName.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            responseContent.Should().NotBeNull();
            responseContent!.Errors.Should().ContainKey("Products[0].Name")
                .WhoseValue.Should().Contain("The Name field is required.");
        }

        [Fact]
        public async Task CreateOrderTest_24_ValidationProductNameMaxLenght_ShouldFail()
        {
            await CleanDatabase();

            var requestProductName = TestData.CreateOrderTestRequest();
            requestProductName.Products[0].Name = new string('A', 201); // Set Product Name with 201 characters

            var responseProductName = await clientHttp.PostAsJsonAsync("v1/orders", requestProductName);
            responseProductName.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseProductName.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("Product name max length is 200");
        }

        [Fact]
        public async Task CreateOrderTest_25_ValidationProductDescriptionMaxLenght_ShouldFail()
        {
            await CleanDatabase();

            var requestProductDescription = TestData.CreateOrderTestRequest();
            requestProductDescription.Products[0].Description = new string('A', 2001); // Set Product Description with 2001 characters

            var responseProductDescription = await clientHttp.PostAsJsonAsync("v1/orders", requestProductDescription);
            responseProductDescription.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseProductDescription.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("Description max length is 1000");
        }

        [Fact]
        public async Task CreateOrderTest_26_ValidationProductPriceGreaterThan0_ShouldFail()
        {
            await CleanDatabase();

            var requestProductPrice = TestData.CreateOrderTestRequest();
            requestProductPrice.Products[0].Price = 0; // Set Product Price to 0

            var responseProductPrice = await clientHttp.PostAsJsonAsync("v1/orders", requestProductPrice);
            responseProductPrice.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseProductPrice.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("Price must be greater than 0");
        }

        [Fact]
        public async Task CreateOrderTest_27_ValidationProductQuantityGreaterThan0_ShouldFail()
        {
            await CleanDatabase();

            var requestProductQuantity = TestData.CreateOrderTestRequest();
            requestProductQuantity.Products[0].Quantity = 0; // Set Product Quantity to 0

            var responseProductQuantity = await clientHttp.PostAsJsonAsync("v1/orders", requestProductQuantity);
            responseProductQuantity.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await responseProductQuantity.Content.ReadAsStringAsync();
            responseContent.Should().NotBeNull();
            responseContent.Should().Contain("Quantity must be at least 1");
        }
    }
}
