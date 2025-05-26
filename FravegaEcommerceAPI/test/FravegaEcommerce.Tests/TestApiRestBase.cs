using FravegaEcommerceAPI.Models.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System.Net.Http.Json;
using Xunit;

namespace FravegaEcommerce.Tests
{
    public abstract class TestApiRestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly CustomWebApplicationFactory factory;
        protected readonly HttpClient clientHttp;

        public TestApiRestBase(CustomWebApplicationFactory _factory)
        {
            factory = _factory;
            clientHttp = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
        }

        protected async Task CleanDatabase()
        {
            var mongoClient = factory.Services.GetRequiredService<IMongoClient>();
            var databaseName = "FravegaDB";
            var database = mongoClient.GetDatabase(databaseName);

            var collections = await database.ListCollectionNames().ToListAsync();
            foreach (var collection in collections)
            {
                await database.DropCollectionAsync(collection);
            }
        }

        protected async Task<(Order? request, HttpResponseMessage response)> CreateNewOrder()
        {
            await CleanDatabase();

            var request = TestData.CreateOrderTestRequest();
            var response = await clientHttp.PostAsJsonAsync("v1/orders", request);
            response.EnsureSuccessStatusCode();

            return (request, response);
        }

        public void Dispose()
        {
            CleanDatabase().Wait();
            clientHttp.Dispose();
        }
    }
}
