using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Unsafe.Data;

public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderID { get; set; }
    public string UserID { get; set; }
    public string BillingPostalCode { get; set; }
    public string CreditCardNumber { get; set; }
    public string CardExpirationMonth { get; set; }
    public string CardExpirationYear { get; set; }
    public string CardCvcNumber { get; set; }
    public double AmountPaid { get; set; }
    public string PaymentID { get; set; }

    public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
