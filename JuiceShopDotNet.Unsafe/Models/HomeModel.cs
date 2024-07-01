using JuiceShopDotNet.Unsafe.Data;

namespace JuiceShopDotNet.Unsafe.Models;

public class HomeModel
{
    public List<Product> Products { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalProductCount { get; set; }
}
