using FravegaEcommerceAPI.Enums;

namespace FravegaEcommerceAPI.Translators
{
    public static class StatusTranslator
    {
        private static readonly Dictionary<OrderStatus, string> Translations = new()
        {
            [OrderStatus.Created] = "Creada",
            [OrderStatus.PaymentReceived] = "Pago recibido",
            [OrderStatus.Cancelled] = "Cancelada",
            [OrderStatus.Invoiced] = "Facturada",
            [OrderStatus.Returned] = "Devuelta"
        };

        public static string Translate(OrderStatus status)
        {
            return Translations.TryGetValue(status, out var translation)
                ? translation
                : status.ToString();
        }
    }
}
