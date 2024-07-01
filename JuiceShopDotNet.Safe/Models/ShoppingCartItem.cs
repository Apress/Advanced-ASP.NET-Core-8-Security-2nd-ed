namespace JuiceShopDotNet.Safe.Models;

public class ShoppingCartItem
{
    public int ProductID { get; set; }
    public float Price { get; set; }
    public int Quantity { get; set; }
    public string ProductName { get; set; }
    public string ImageName { get; set; }
}
