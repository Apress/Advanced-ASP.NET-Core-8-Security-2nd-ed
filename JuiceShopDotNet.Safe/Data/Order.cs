using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using JuiceShopDotNet.Safe.Data.ExpressionFilters;

namespace JuiceShopDotNet.Safe.Data;

public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderID { get; set; }

    [UserIdentifier]
    public int JuiceShopUserID { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? CreditCardLastFour { get; set; }
    public double? AmountPaid { get; set; }
    public string? PaymentID { get; set; }
    public DateTime? OrderCompletedOn { get; set; }

    public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
