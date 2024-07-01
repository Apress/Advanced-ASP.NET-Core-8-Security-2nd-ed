using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Safe.Data;

public class OrderProduct
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderProductID { get; set; }
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public float ProductPrice { get; set; }
    public int Quantity { get; set; }

    public Order Order { get; set; }
    public Product Product { get; set; }
}
