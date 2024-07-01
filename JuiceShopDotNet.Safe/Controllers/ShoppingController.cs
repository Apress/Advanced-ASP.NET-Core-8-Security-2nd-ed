using JuiceShopDotNet.Common.PaymentProcessor;
using JuiceShopDotNet.Safe.Auth;
using JuiceShopDotNet.Safe.Data;
using JuiceShopDotNet.Safe.Data.Extensions;
using JuiceShopDotNet.Safe.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JuiceShopDotNet.Safe.Controllers;

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

    [ValidateAntiForgeryToken]
    [HttpPost]
    public IActionResult AddToCart([FromForm]AddToCartModel item)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("Error", "Home");

        var userOrder = _dbContext.Orders.GetOpenOrder(User, false);

        var existingCartItem = userOrder.OrderProducts.SingleOrDefault(i => i.ProductID == item.ProductID);

        if (existingCartItem == null)
        {
            existingCartItem = new OrderProduct();
            existingCartItem.ProductID = item.ProductID;
            userOrder.OrderProducts.Add(existingCartItem);
        }
        else
        {
            var newQuantity = existingCartItem.Quantity + item.Quantity;
            existingCartItem.Quantity = newQuantity;
        }

        var product = _dbContext.Products.SingleOrDefault(p => p.id == item.ProductID);

        //Ensure that the price for these items is the price that was valid at the time of order
        //Note that in a "real" e-commerce system, we would notify the user if the price had changed
        existingCartItem.ProductPrice = product.displayPrice;
        existingCartItem.Quantity = existingCartItem.Quantity + item.Quantity;
        _dbContext.SaveChanges();

        var model = new AddToCartDisplayModel();
        model.OrderProduct = existingCartItem;
        model.Product = product;

        //It's not a good idea to get into the habit of returning EF objects directly to the UI
        //If the schema is exposed, it may give attackers information to better pull off overposting attacks
        //In our case, though, we're just returning the item to the view so no information is exposed
        return View(model);
    }

    [HttpGet]
    public IActionResult Review()
    {
        var userOrder = _dbContext.Orders.GetOpenOrder(User, true);
        return View(userOrder);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public IActionResult Review([FromForm]ShoppingCartReviewModel model)
    {
        var userOrder = _dbContext.Orders.GetOpenOrder(User, true);

        if (!ModelState.IsValid)
        {
            return View(userOrder);
        }

        foreach (var key in model.Quantity.Keys)
        {
            var orderProduct = userOrder.OrderProducts.SingleOrDefault(op => op.ProductID == key);

            if (orderProduct == null)
            {
                ModelState.AddModelError("Product Not Found", "There was an error during the review process. Please try again.");
                return View(userOrder);
            }

            orderProduct.Quantity = model.Quantity[key];
        }

        _dbContext.SaveChanges();

        return RedirectToAction(nameof(Checkout));
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        var order = _dbContext.Orders.GetOpenOrder(User, false);
        ViewBag.Total = Convert.ToSingle(Math.Round(order.OrderProducts.Sum(op => op.ProductPrice * op.Quantity), 2));
        return View(new CheckoutModel());
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public IActionResult Checkout([FromForm]CheckoutModel model)
    {
        var order = _dbContext.Orders.GetOpenOrder(User, false);
        var amount = Convert.ToSingle(Math.Round(order.OrderProducts.Sum(op => op.ProductPrice * op.Quantity), 2));

        ViewBag.Total = amount;

        if (!ModelState.IsValid)
            return View(model);

        var paymentInfo = new PaymentInfo()
        {
            BillingPostalCode = model.BillingPostalCode,
            CreditCardNumber = model.CreditCardNumber,
            CardExpirationMonth = model.CardExpirationMonth,
            CardExpirationYear = model.CardExpirationYear.ToString(),
            CardCvcNumber = model.CardCvcNumber,
            AmountToCharge = amount
        };

        var result = PaymentSimulator.Pay(paymentInfo);

        if (result.Result == PaymentResult.ActualResult.Succeeded)
        {
            order.PaymentID = result.PaymentID.Value.ToString();
            order.CreditCardLastFour = model.CreditCardNumber.Substring(model.CreditCardNumber.Length - 4);
            order.BillingPostalCode = model.BillingPostalCode;
            order.AmountPaid = amount;
            order.OrderCompletedOn = DateTime.Now;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Completed));
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error, "");
            }

            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Completed()
    {
        return View();
    }

    [HttpGet]
    public IActionResult History()
    {
        var orders = _dbContext.FilterByUser(User).Orders.Where(o => o.PaymentID != null).OrderByDescending(o => o.OrderCompletedOn).ToList();
        return View(orders);
    }

    [AuthorizeOrder("id")]
    [HttpGet]
    public IActionResult Details([FromRoute]int id)
    {
        var order = _dbContext.FilterByUser(User).Orders.Include(o => o.OrderProducts).ThenInclude(op => op.Product).Single(o => o.OrderID == id);
        return View(order);
    }
}
