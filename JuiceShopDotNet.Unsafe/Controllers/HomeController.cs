using JuiceShopDotNet.Unsafe.Data;
using JuiceShopDotNet.Unsafe.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JuiceShopDotNet.Unsafe.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public IActionResult Index(int page, int pageSize)
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

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
