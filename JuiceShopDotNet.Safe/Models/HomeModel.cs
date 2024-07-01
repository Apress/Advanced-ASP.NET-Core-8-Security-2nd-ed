using JuiceShopDotNet.Safe.Data;

namespace JuiceShopDotNet.Safe.Models;

public class HomeModel
{
    public List<Product> Products { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalProductCount { get; set; }
}
