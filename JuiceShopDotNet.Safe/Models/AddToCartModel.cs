using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Safe.Models;

public class AddToCartModel
{
    [Required]
    public int ProductID { get; set; }

    [Required]
    public int Quantity { get; set; }
}
