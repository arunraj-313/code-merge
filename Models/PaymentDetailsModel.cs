using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace StayEasePG.Models
{
    // This model will capture payment info if not 'Cash on Hand'
    public class PaymentDetailsModel
    {
        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }
        // --- Card Details (Used only if method is Credit Card) ---
        [Display(Name = "Card Holder Name")]
        public string CardHolderName { get; set; }
        [CreditCard]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }
        [Display(Name = "Expiration Date")]
        public string ExpirationDate { get; set; } // Format: MM/YY
        [Range(100, 9999)]
        public int CVV { get; set; }
        // --- Fields inherited from the previous step ---
        public BookingModel BookingData { get; set; }
    }
}