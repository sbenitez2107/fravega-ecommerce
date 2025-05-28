using FravegaEcommerceAPI.Enums;
using FravegaEcommerceAPI.Models.DTOs.Requests;
using FravegaEcommerceAPI.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FravegaEcommerce.Tests
{
    public static class TestData
    {
        public static Order CreateOrderTestRequest()
        {
            var request = new CreateOrderRequest(
                ExternalReferenceId: "EC-0000000001",
                Channel: "Ecommerce",
                PurchaseDate: DateTime.UtcNow,
                TotalValue: 8000,
                Buyer: new BuyerDto(
                    FirstName: "Juan",
                    LastName: "Perez",
                    DocumentNumber: "123456789",
                    Phone: "+541143345678"),
                Products: new List<ProductDto>
                {
                    new ProductDto(
                        Sku: "P001",
                        Name: "Product A",
                        Description: "Description A",
                        Price: 1000m,
                        Quantity: 2),
                    new ProductDto(
                        Sku: "P002",
                        Name: "Product B",
                        Description: "Description B",
                        Price: 1500m,
                        Quantity: 4)
                    });

            var newOrder = new Order
            {
                OrderId = 1,
                ExternalReferenceId = request.ExternalReferenceId,
                Channel = request.Channel,
                PurchaseDate = request.PurchaseDate,
                TotalValue = request.TotalValue,
                Buyer = new Buyer()
                {
                    FirstName = request.Buyer.FirstName,
                    LastName = request.Buyer.LastName,
                    DocumentNumber = request.Buyer.DocumentNumber,
                    Phone = request.Buyer.Phone
                },
                Products = request.Products.Select(x => new Product
                {
                    Sku = x.Sku,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Quantity = x.Quantity
                }).ToList(),
                Status = OrderStatus.Created,
                Events = new List<Event>
                {
                    new Event
                    {
                        Id = "event-001",
                        Type = OrderStatus.Created.ToString(),
                        Date = DateTime.UtcNow
                    }
                }
            };

            return newOrder;
        } 
    
        public static AddEventRequest CreateAddEventRequest()
        {
            return new AddEventRequest(
                Id: "event-002",
                Type: "PaymentReceived",
                Date: DateTime.UtcNow,
                User: "externalUser"
            );
        }
    }
}
