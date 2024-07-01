using JuiceShopDotNet.Safe.Data;
using System.ComponentModel.DataAnnotations;

namespace JuiceShopDotNet.Safe.Models;

public class EditReviewModel
{
    public EditReviewModel() { }
    public EditReviewModel(ProductReview review)
    { 
        ProductReviewID = review.ProductReviewID;
        ReviewText = review.ReviewText;
    }

    [Required]
    public int ProductReviewID { get; set; }

    [Required]
    public string ReviewText { get; set; }
}
