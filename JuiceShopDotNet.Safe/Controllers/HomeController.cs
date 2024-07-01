using JuiceShopDotNet.Safe.Data;
using JuiceShopDotNet.Safe.Logging;
using JuiceShopDotNet.Safe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JuiceShopDotNet.Safe.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly ISecurityLogger _securityLogger;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, ISecurityLoggerFactory securityLoggerFactory)
    {
        _logger = logger;
        _dbContext = dbContext;
        _securityLogger = securityLoggerFactory.CreateLogger<HomeController>();
    }

    [HttpGet]
    public IActionResult Index([FromQuery]int page, [FromQuery]int pageSize)
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

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult About()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location =
      ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var context = HttpContext.Features.
          Get<IExceptionHandlerFeature>();

        var requestId = Activity.Current?.Id ??
          HttpContext.TraceIdentifier;

        //Use the error logger instead
        //_securityLogger.Log(SecurityEvent.General.EXCEPTION, $"An error occurred, request ID: {requestId}, error: {context.Error}");

        return View(new ErrorViewModel { RequestId = requestId });
    }

    [HttpGet]
    public IActionResult TestErrorHandling()
    {
        throw new NotImplementedException();
    }
}
