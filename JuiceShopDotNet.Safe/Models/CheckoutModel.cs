using JuiceShopDotNet.Safe.Validators;
using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Safe.Models;

public class CheckoutModel
{
    [Required]
    [StringLength(20)]
    public string BillingPostalCode { get; set; }

    [Required]
    [CreditCard]
    public string CreditCardNumber { get; set; }

    [CreditCardMonth]
    [Required]
    public string CardExpirationMonth { get; set; }

    [Required]
    [Range(2024, 2060)]
    public int CardExpirationYear { get; set; }

    [CreditCardCvc]
    [Required]
    public string CardCvcNumber { get; set; }
}
