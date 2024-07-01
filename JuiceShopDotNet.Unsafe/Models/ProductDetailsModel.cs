using JuiceShopDotNet.Unsafe.Data;

namespace JuiceShopDotNet.Unsafe.Models;

public class ProductDetailsModel
{
    public Product Product { get; set; }
    public List<ProductReview_Display> ProductReviews { get; set; }
}
