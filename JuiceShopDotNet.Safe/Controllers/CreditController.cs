using JuiceShopDotNet.Safe.Data;
using JuiceShopDotNet.Safe.Models;
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
        var userID = int.Parse(HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
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
    public IActionResult Apply([FromForm]CreditApplicationModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var newApp = new CreditApplication();
        newApp.UserID = int.Parse(HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        newApp.FullName = model.FullName;
        newApp.Birthdate = model.Birthdate;
        newApp.SocialSecurityNumber = model.SocialSecurityNumber;
        newApp.EmploymentStatus = model.EmploymentStatus;
        newApp.Income = model.Income;
        newApp.SubmittedOn = DateTime.UtcNow;

        _dbContext.Add(newApp);
        _dbContext.SaveChanges();

        return RedirectToAction("Index");
    }
}
