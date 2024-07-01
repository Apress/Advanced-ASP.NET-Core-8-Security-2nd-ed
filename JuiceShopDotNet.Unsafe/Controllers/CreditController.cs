using JuiceShopDotNet.Unsafe.Data;
using JuiceShopDotNet.Unsafe.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace JuiceShopDotNet.Safe.Controllers;

[Authorize]
public class CreditController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public CreditController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var userID = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var creditApplication = _dbContext.CreditApplications.SingleOrDefault(c => c.UserID == userID);
        return View(creditApplication);
    }

    [HttpGet]
    public IActionResult Apply()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Apply([FromForm]CreditApplication model)
    {
        model.UserID = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
        model.SubmittedOn = DateTime.UtcNow;

        _dbContext.Add(model);
        _dbContext.SaveChanges();

        return RedirectToAction("Index");
    }
}
