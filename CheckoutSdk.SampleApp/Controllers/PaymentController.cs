using System;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using Checkout;
using Checkout.Common;
using Checkout.Payments;
using CheckoutSdk.SampleApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CheckoutSdk.SampleApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ICheckoutApi _checkoutApi;

        public PaymentController(ICheckoutApi checkoutApi)
        {
            this._checkoutApi = checkoutApi;
        }

        public IActionResult Index()
        {
            ViewData["Currency"] = new[] {
                new SelectListItem(){Value = Currency.USD,Text = Currency.USD},
                new SelectListItem(){Value = Currency.EUR,Text = Currency.EUR},
                new SelectListItem(){Value = Currency.GBP,Text = Currency.GBP}};
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestPayment(PaymentModel model)
        {
            try
            {


                if (string.IsNullOrWhiteSpace(model.CardToken))
                    throw new ArgumentException("Model", $"{nameof(model.CardToken)} is missing.");

                if (string.IsNullOrWhiteSpace(model.Currency) || model.Amount == null || model.Amount < 1)
                    throw new ArgumentException("Model", $"Please fill all the missing fields.");

                var source = new TokenSource(model.CardToken);
                var paymentRequest = new PaymentRequest<TokenSource>(source, model.Currency, model.Amount);

                var response = await _checkoutApi.Payments.RequestAsync(paymentRequest);

                if (response.IsPending)
                {
                    return Redirect(response.Pending.GetRedirectLink().ToString());
                }

                return View("Success", response.Payment);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Model", e.ToString());
                return View(nameof(Error));
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
