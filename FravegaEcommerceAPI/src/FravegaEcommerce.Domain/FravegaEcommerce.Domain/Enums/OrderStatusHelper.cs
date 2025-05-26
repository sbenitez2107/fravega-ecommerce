namespace FravegaEcommerceAPI.Enums
{
    public static class OrderStatusHelper
    {
        public static OrderStatus FromString(string value)
        {
            return Enum.TryParse<OrderStatus>(value, true, out var result)
                ? result
                : throw new ArgumentException($"Invalid OrderStatus value: {value}");
        }

        public static bool TryFromString(string value, out OrderStatus status)
        {
            return Enum.TryParse(value, true, out status);
        }
    }
}
