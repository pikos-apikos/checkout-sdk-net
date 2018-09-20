using System;
using Checkout.Common;
using Checkout.Payments;
using Checkout.Tests.Mocks;
using Checkout.Tokens;

namespace Checkout.Tests
{
    public static class TestHelper
    {
        public static PaymentRequest<CardSource> CreateCardPaymentRequest()
        {
            return new PaymentRequest<CardSource>(
                new CardSource(TestCardSource.Visa.Number, TestCardSource.Visa.ExpiryMonth, TestCardSource.Visa.ExpiryYear),
                Currency.GBP,
                100
            )
            {
                Capture = false,
                Customer = new Customer() { Email = TestHelper.GenerateRandomEmail()},
                Reference = Guid.NewGuid().ToString()
            };
        }

        public static CardTokenRequest CreateCardTokenRequest()
        {
            return new CardTokenRequest(TestCardSource.Visa.Number, TestCardSource.Visa.ExpiryMonth,
                TestCardSource.Visa.ExpiryYear)
            {
                Cvv = TestCardSource.Visa.Cvv
            };
        }

        public static string GenerateRandomEmail()
        {
            return Guid.NewGuid().ToString("n") + "@checkout-sdk-net.com";
        }
    }
}