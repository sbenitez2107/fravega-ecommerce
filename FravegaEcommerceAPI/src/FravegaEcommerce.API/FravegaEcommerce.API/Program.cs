using FluentValidation;
using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Mappers;
using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Repositories;
using FravegaEcommerceAPI.Services;
using FravegaEcommerceAPI.Validators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using FravegaEcommerceAPI.Models.Entities;
using Microsoft.OpenApi.Models;

public class Program()
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Database
        var connectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://admin:fravega@localhost:27017/?authSource=admin";
        var mongoClient = new MongoClient(connectionString);
        var database = mongoClient.GetDatabase("FravegaDB");
        builder.Services.AddSingleton<IMongoDatabase>(database);

        // MongoDB convertions
        BsonSerializer.RegisterSerializer(new EnumSerializer<OrderStatus>(BsonType.String));

        if (!BsonClassMap.IsClassMapRegistered(typeof(Order)))
        {
            BsonClassMap.RegisterClassMap<Order>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(o => o.Status).SetSerializer(new EnumSerializer<OrderStatus>(BsonType.String));
            });
        }

        // Services
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderService, OrderService>();

        // Controllers
        builder.Services.AddControllers();

        // AutoMapper
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(OrderMapper));
            cfg.ShouldUseConstructor = constructor => constructor.IsPublic;
        });

        // Validators
        builder.Services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderValidator>();
        builder.Services.AddScoped<IValidator<AddEventRequest>, AddEventValidator>();

        builder.Services.AddEndpointsApiExplorer();

        // swagger
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Fravega Ecommerce API",
                Description = "API for orders",
                Version = "v1",
                Contact = new OpenApiContact { Name = "Development Team" }
            });
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();
        app.UseHttpsRedirection();
        app.UseExceptionHandler("/error");

        app.Run();
    }
}
