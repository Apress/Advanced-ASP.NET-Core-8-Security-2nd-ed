using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace JuiceShopDotNet.Unsafe.Data;

public class ProductReview
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductReviewID { get; set; }
    public int ProductID { get; set; }
    public string UserID { get; set; }
    public string ReviewText { get; set; }

    public Product Product { get; set; }
    public IdentityUser User { get; set; }
}
