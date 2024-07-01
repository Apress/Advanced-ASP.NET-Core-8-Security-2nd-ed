using JuiceShopDotNet.Safe.Data;

namespace JuiceShopDotNet.Safe.Models;

public class AddToCartDisplayModel
{
    public Product Product { get; set; }
    public OrderProduct OrderProduct { get; set; }
}
