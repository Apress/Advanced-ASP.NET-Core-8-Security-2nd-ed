using JuiceShopDotNet.Safe.Auth;
using JuiceShopDotNet.Safe.Data;
using JuiceShopDotNet.Safe.Data.ExpressionFilters;
using JuiceShopDotNet.Safe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JuiceShopDotNet.Safe.Controllers;

[AutoValidateAntiforgeryToken]
public class ProductController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<JuiceShopUser> _userManager;

    public ProductController(ApplicationDbContext dbContext, UserManager<JuiceShopUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Index([FromQuery] int page, [FromQuery] int pageSize)
    {
        if (pageSize <= 0)
            pageSize = 12;

        if (page <= 1)
            page = 1;

        var model = new HomeModel();
        model.Products = _dbContext.Products.OrderBy(p => p.name).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        model.PageNumber = page;
        model.PageSize = pageSize;
        model.TotalProductCount = _dbContext.Products.Count();

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Details(int id)
    {
        var model = new ProductDetailsModel();
        model.Product = _dbContext.Products.FirstOrDefault(p => p.id == id);
        model.ProductReviews = _dbContext.ProductReviews.Where(p => p.ProductID == id).ToList();

        foreach (var review in model.ProductReviews)
        {
            review.CreatedBy = _userManager.FindByIdAsync(review.JuiceShopUserID.ToString()).Result.DisplayName;
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult AddReview(int id, string reviewText)
    {
        var newReview = new ProductReview();
        newReview.ProductID = id;
        newReview.ReviewText = reviewText;
        newReview.JuiceShopUserID = User.GetUserID();
        newReview.CreatedOn = DateTime.UtcNow;

        _dbContext.ProductReviews.Add(newReview);
        _dbContext.SaveChanges();

        return RedirectToAction("Details", "Product", new { id });
    }

    [HttpGet]
    public IActionResult MyReviews()
    {
        var reviews = _dbContext.ProductReviews.Include(r => r.Product).ForUser(User).ToList();
        return View(reviews);
    }


    [HttpGet]
    public IActionResult Review([FromRoute]int id)
    {
        var review = _dbContext.ProductReviews.SingleForUser(User, r => r.ProductReviewID == id);
        var model = new EditReviewModel(review);
        return View(model);
    }

    [HttpPost]
    public IActionResult Review([FromForm] EditReviewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var review = _dbContext.ProductReviews.SingleForUser(User, r => r.ProductReviewID == model.ProductReviewID);
        review.ReviewText = model.ReviewText;
        _dbContext.SaveChanges();

        return RedirectToAction(nameof(MyReviews));
    }
}
