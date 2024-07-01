using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using JuiceShopDotNet.Safe.Data.ExpressionFilters;

namespace JuiceShopDotNet.Safe.Data;

public class ProductReview
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductReviewID { get; set; }
    public int ProductID { get; set; }

    [UserIdentifier]
    public int JuiceShopUserID { get; set; }
    public string ReviewText { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    [NotMapped]
    public string CreatedBy { get; set; }

    public Product Product { get; set; }
}
