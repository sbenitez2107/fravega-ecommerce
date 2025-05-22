using FravegaEcommerceAPI.Enums;

namespace FravegaEcommerceAPI.Transitions
{
    public static class StateTransitions
    {
        private static readonly Dictionary<OrderStatus, List<OrderStatus>> AllowedTransitions = new()
        {
            [OrderStatus.Created] = new() { OrderStatus.PaymentReceived, OrderStatus.Canceled },
            [OrderStatus.PaymentReceived] = new() { OrderStatus.Invoiced },
            [OrderStatus.Invoiced] = new() { OrderStatus.Returned }
        };

        public static bool IsValidTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return AllowedTransitions.TryGetValue(currentStatus, out var allowed)
                && allowed.Contains(newStatus);
        }
    }
}
