using Microsoft.Extensions.Configuration;

namespace Checkout.Sdk.Microsoft.Extensions
{
    public static class CheckoutConfigurationExtensions
    {
        public static CheckoutOptions GetCheckoutOptions(this IConfiguration configuration)
        {
            return configuration.GetSection("Checkout").Get<CheckoutOptions>();
        }
    }
}