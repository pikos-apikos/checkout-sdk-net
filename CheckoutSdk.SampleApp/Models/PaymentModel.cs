using System.ComponentModel.DataAnnotations;

namespace CheckoutSdk.SampleApp.Models
{
    public class PaymentModel
    {
        [Required]
        public int? Amount { get; set; }
        [Required]
        public string Currency { get; set; }
        [Required]
        public string CardToken { get; set; }
        [Required]
        public bool Do3ds { get; set; }
    }
}
