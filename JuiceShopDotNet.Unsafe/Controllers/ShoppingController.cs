using JuiceShopDotNet.Common.PaymentProcessor;
using JuiceShopDotNet.Unsafe.Data;
using JuiceShopDotNet.Unsafe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace JuiceShopDotNet.Unsafe.Controllers;

public class ShoppingController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ShoppingController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        throw new NotImplementedException();
    }

    [Authorize]
    public IActionResult AddToCart(ShoppingCartItem item)
    {
        List<ShoppingCartItem> cart = GetShoppingCart();

        var existingCartItem = cart.SingleOrDefault(i => i.ProductID == item.ProductID);

        if (existingCartItem == null)
            cart.Add(item);
        else
        {
            var newQuantity = existingCartItem.Quantity + item.Quantity;
            cart.Single(i => i.ProductID == item.ProductID).Quantity = newQuantity;
            item.Quantity = newQuantity;
        }

        var cartItem = cart.Single(i => i.ProductID == item.ProductID);
        var product = _dbContext.Products.SingleOrDefault(p => p.id == item.ProductID);

        cartItem.Price = item.Price;
        cartItem.ProductName = product.name;
        cartItem.ImageName = product.image;

        var cookieOptions = new CookieOptions();
        cookieOptions.SameSite = SameSiteMode.None;
        cookieOptions.HttpOnly = false;
        cookieOptions.Secure = false;

        Response.Cookies.Delete("ShoppingCart");
        Response.Cookies.Append("ShoppingCart", JsonSerializer.Serialize(cart), cookieOptions);

        var model = new AddToCartModel();
        model.ShoppingCartItem = item;
        model.Product = product;

        return View(model);
    }

    public IActionResult Review()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        var order = new Order();
        order.UserID = User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
        order.AmountPaid = Convert.ToSingle(Math.Round(GetShoppingCart().Sum(c => c.Quantity * c.Price), 2));
        return View(order);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Checkout(Order order)
    {
        var paymentInfo = new PaymentInfo()
        { 
            BillingPostalCode = order.BillingPostalCode,
            CreditCardNumber = order.CreditCardNumber,
            CardExpirationMonth = order.CardExpirationMonth,
            CardExpirationYear = order.CardExpirationYear,
            CardCvcNumber = order.CardCvcNumber,
            AmountToCharge = order.AmountPaid
        };

        var result = PaymentSimulator.Pay(paymentInfo);

        if (result.Result == PaymentResult.ActualResult.Succeeded)
        {
            order.PaymentID = result.PaymentID.Value.ToString();

            _dbContext.Orders.Add(order);

            foreach (var product in GetShoppingCart())
            {
                var orderProduct = new OrderProduct();
                orderProduct.ProductPrice = product.Price;
                orderProduct.ProductID = product.ProductID;
                orderProduct.Quantity = product.Quantity;
                order.OrderProducts.Add(orderProduct);
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Completed");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error, "");
            }

            return View(order);
        }
    }

    public IActionResult Completed()
    {
        return View();
    }

    [HttpGet]
    public IActionResult History()
    {
        var userID = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var orders = _dbContext.Orders.Where(o => o.PaymentID != null && o.UserID == userID).OrderByDescending(o => o.OrderID).ToList();
        return View(orders);
    }

    [HttpGet]
    public IActionResult Details([FromRoute] int id)
    {
        var order = _dbContext.Orders.Include(o => o.OrderProducts).ThenInclude(op => op.Product).Single(o => o.OrderID == id);
        return View(order);
    }

    private List<ShoppingCartItem> GetShoppingCart()
    {
        var cartAsString = Request.Cookies["ShoppingCart"];
        var cart = new List<ShoppingCartItem>();

        if (!string.IsNullOrEmpty(cartAsString))
            cart = System.Text.Json.JsonSerializer.Deserialize<List<ShoppingCartItem>>(cartAsString);

        return cart;
    }
}
