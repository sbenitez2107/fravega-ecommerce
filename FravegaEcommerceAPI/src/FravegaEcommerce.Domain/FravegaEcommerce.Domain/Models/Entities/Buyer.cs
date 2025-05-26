namespace FravegaEcommerceAPI.Models.Entities
{
    public class Buyer
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string DocumentNumber { get; set; }
        public string? Phone { get; set; }
    }
}
