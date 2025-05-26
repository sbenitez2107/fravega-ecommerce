namespace FravegaEcommerceAPI.Translators
{
    public static class ChannelTranslator
    {
        private static readonly Dictionary<string, string> Translations = new()
        {
            ["Ecommerce"] = "Comercio electronico",
            ["CallCenter"] = "Centro de llamadas",
            ["Store"] = "Tienda fisica",
            ["Affiliate"] = "Afiliado"
        };

        public static string Translate(string channel)
        {
            return Translations.TryGetValue(channel, out var translation)
                ? translation
                : channel; // Retuns the original value if no translation is found
        }
    }
}
