using JuiceShopDotNet.Unsafe.Data;

namespace JuiceShopDotNet.Unsafe.Models;

public class AddToCartModel
{
    public Product Product { get; set; }
    public ShoppingCartItem ShoppingCartItem { get; set; }
}
