using FravegaEcommerceAPI.Enums;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FravegaEcommerceAPI.Models.Entities
{
    public class Order
    {
        [BsonId]
        public int OrderId { get; set; }
        [BsonElement("externalReferenceId")]
        public required string ExternalReferenceId { get; set; }
        [BsonElement("channel")]
        public required string Channel { get; set; }
        [BsonElement("purchaseDate")]
        public DateTime PurchaseDate { get; set; }
        [BsonElement("totalValue")]
        public decimal TotalValue { get; set; }
        [BsonElement("buyer")]
        public required Buyer Buyer { get; set; }
        [BsonElement("products")]
        public required List<Product> Products { get; set; }
        [BsonElement("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedOn { get; set; }
        [BsonElement("events")]
        public List<Event> Events { get; set; } = new List<Event>();
    }
}
