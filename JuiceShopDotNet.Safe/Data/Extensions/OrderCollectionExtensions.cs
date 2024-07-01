using JuiceShopDotNet.Safe.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Security.Claims;

namespace JuiceShopDotNet.Safe.Data.Extensions;

public static class OrderCollectionExtensions
{
    public static Order GetOpenOrder(this DbSet<Order> orders, ClaimsPrincipal principal, bool includeProducts)
    {
        var userID = principal.GetUserID();

        IQueryable<Order> query;

        if (includeProducts)
            query = orders.Include(o => o.OrderProducts).ThenInclude(op => op.Product);
        else
            query = orders.Include(o => o.OrderProducts);

        var order = query.SingleOrDefault(o => o.JuiceShopUserID == userID && !o.OrderCompletedOn.HasValue);

        if (order == null) 
        {
            order = new Order();
            order.JuiceShopUserID = userID;
            orders.Add(order);
        }

        return order;
    }
}
