using MongoDB.Bson.Serialization.Attributes;

namespace FravegaEcommerceAPI.Models.Entities
{
    public class Counter
    {
        [BsonId]
        public required string Id { get; set; }
        public int Sequence { get; set; }
    }
}
