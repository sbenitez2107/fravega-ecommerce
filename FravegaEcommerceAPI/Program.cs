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

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://admin:fravega@localhost:27017/?authSource=admin";
var mongoClient = new MongoClient(connectionString);
var database = mongoClient.GetDatabase("FravegaDB");
builder.Services.AddSingleton<IMongoDatabase>(database);

// MongoDB convertions
BsonSerializer.RegisterSerializer(new EnumSerializer<OrderStatus>(BsonType.String));

// Services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Controllers
builder.Services.AddControllers();

// AutoMapper
builder.Services.AddAutoMapper(typeof(OrderMapper));

// Validators
builder.Services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderValidator>();
builder.Services.AddScoped<IValidator<AddEventRequest>, AddEventValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
