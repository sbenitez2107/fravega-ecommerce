using AutoMapper;
using FravegaEcommerceAPI.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace FravegaEcommerce.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public IMapper Mapper { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<OrderMapper>());

            Mapper = config.CreateMapper();

            IConfiguration Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IMongoClient>(_ => new MongoClient(Configuration["ConnectionStrings:MongoDB"]));
            });

            builder.ConfigureServices(services =>
            {
                services.AddSingleton(Mapper);
            });
        }
    }
}
