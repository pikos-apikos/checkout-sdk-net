using System;
using System.Threading.Tasks;
using Checkout.Sdk.Common;
using Checkout.Sdk.Payments;
using Checkout.Sdk.Tests.Mocks;
using Shouldly;
using Xunit;

namespace Checkout.Sdk.Tests
{
    public class PaymentsClientTests : IClassFixture<ApiTestFixture>
    {
        public PaymentsClientTests(ApiTestFixture fixture)
        {
            Api = fixture.Api;
        }

        public ICheckoutApi Api { get; private set; }

        [Fact]
        public async Task RequestNonThreeDsCardPayment()
        {
            PaymentRequest<CardSource> paymentRequest = CreateCardPaymentRequest();
            paymentRequest.ThreeDs = false;

            PaymentResponse<CardSourceResponse> apiResponse = await Api.Payments.RequestAsync(paymentRequest);
            
            apiResponse.Payment.ShouldNotBeNull();
            apiResponse.Payment.Approved.ShouldBeTrue();
            apiResponse.Payment.Id.ShouldNotBeNullOrEmpty();
            apiResponse.Payment.ActionId.ShouldNotBeNullOrEmpty();
            apiResponse.Payment.Amount.ShouldBe(paymentRequest.Amount.Value);
            apiResponse.Payment.Currency.ShouldBe(paymentRequest.Currency);
            apiResponse.Payment.Reference.ShouldBe(paymentRequest.Reference);
            apiResponse.Payment.Customer.ShouldNotBeNull();
            apiResponse.Payment.Customer.Id.ShouldNotBeNullOrEmpty();
            apiResponse.Payment.Customer.Email.ShouldNotBeNullOrEmpty();
            apiResponse.Payment.CanCapture().ShouldBeTrue();
            apiResponse.Payment.CanVoid().ShouldBeTrue();
        }

        private PaymentRequest<CardSource> CreateCardPaymentRequest()
        {
            return new PaymentRequest<CardSource>(
                new CardSource(TestCard.Visa.Number, TestCard.Visa.ExpiryMonth, TestCard.Visa.ExpiryYear),
                Currency.GBP,
                100
            )
            {
                Capture = false,
                Customer = new Customer { Email = TestHelper.GenerateRandomEmail() },
                Reference = Guid.NewGuid().ToString()
            };
        }

        [Fact]
        public async Task RequestThreeDsCardPayment()
        {
            PaymentRequest<CardSource> paymentRequest = CreateCardPaymentRequest();
            paymentRequest.ThreeDs = true;

            PaymentResponse<CardSourceResponse> apiResponse = await Api.Payments.RequestAsync(paymentRequest);

            apiResponse.IsPending.ShouldBe(true);
            var pending = apiResponse.Pending;

            pending.ShouldNotBeNull();

            pending.Id.ShouldNotBeNullOrEmpty();
            pending.Reference.ShouldBe(paymentRequest.Reference);
            pending.Customer.ShouldNotBeNull();
            pending.Customer.Id.ShouldNotBeNullOrEmpty();
            pending.Customer.Email.ShouldBe(paymentRequest.Customer.Email);
            pending.ThreeDs.ShouldNotBeNull();
            pending.ThreeDs.Downgraded.ShouldBe(false);
            pending.ThreeDs.Enrolled.ShouldNotBeNullOrEmpty();
            pending.RequiresRedirect().ShouldBe(true);
            pending.GetRedirectLink().ShouldNotBeNull();
        }

        [Fact]
        public async Task ItCanCapturePayment()
        {
            // Auth
            var paymentRequest = CreateCardPaymentRequest();
            var paymentResponse = await Api.Payments.RequestAsync(paymentRequest);
            paymentResponse.Payment.CanCapture().ShouldBe(true);

            CaptureRequest captureRequest = new CaptureRequest
            {
                Reference = Guid.NewGuid().ToString()
            };

            // Capture
            var captureResponse = await Api.Payments.CaptureAsync(paymentResponse.Payment.Id, captureRequest);

            captureResponse.ActionId.ShouldNotBeNullOrEmpty();
            captureResponse.Reference.ShouldBe(captureRequest.Reference);
        }

        [Fact]
        public async Task ItCanVoidPayment()
        {
            // Auth
            var paymentRequest = CreateCardPaymentRequest();
            var paymentResponse = await Api.Payments.RequestAsync(paymentRequest);
            paymentResponse.Payment.CanVoid().ShouldBe(true);

            VoidRequest voidRequest = new VoidRequest
            {
                Reference = Guid.NewGuid().ToString()
            };

            // Void Auth
            var voidResponse = await Api.Payments.VoidAsync(paymentResponse.Payment.Id, voidRequest);

            voidResponse.ActionId.ShouldNotBeNullOrEmpty();
            voidResponse.Reference.ShouldBe(voidRequest.Reference);
        }

        [Fact]
        public async Task ItCanRefundPayment()
        {
            // Auth
            var paymentRequest = CreateCardPaymentRequest();
            var paymentResponse = await Api.Payments.RequestAsync(paymentRequest);
            paymentResponse.Payment.CanCapture().ShouldBe(true);

            // Capture
            var captureResponse = await Api.Payments.CaptureAsync(paymentResponse.Payment.Id);

            var refundRequest = new RefundRequest
            {
                Reference = Guid.NewGuid().ToString()
            };

            // Refund

            var refundResponse = await Api.Payments.RefundAsync(paymentResponse.Payment.Id, refundRequest);

            refundResponse.ActionId.ShouldNotBeNullOrEmpty();
            refundResponse.Reference.ShouldBe(refundRequest.Reference);
        }

        [Fact]
        public async Task ItCanGetPayment()
        {
            var paymentRequest = CreateCardPaymentRequest();
            var paymentResponse = await Api.Payments.RequestAsync(paymentRequest);

            var paymentDetails = await Api.Payments.GetAsync(paymentResponse.Payment.Id);
            paymentDetails.ShouldNotBeNull();
        }
    }
}