using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Unsafe.Data;

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public float price { get; set; }
    public float? deluxePrice { get; set; }
    public string image { get; set; }

    [NotMapped]
    public float displayPrice
    {
        get
        {
            if (deluxePrice.HasValue)
                return deluxePrice.Value;
            else
                return price;
        }
    }

    public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
}
