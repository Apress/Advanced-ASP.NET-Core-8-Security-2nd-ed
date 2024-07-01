using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;

namespace JuiceShopDotNet.Safe.Data.HardCodedFilters;

public class UserFilter
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ClaimsPrincipal _user;

    public UserFilter(ApplicationDbContext applicationDbContext, ClaimsPrincipal user)
    {
        this._dbContext = applicationDbContext;
        this._user = user;
    }

    public IQueryable<CreditApplication> CreditApplications
    {
        get 
        { 
            var userID = GetUserID();
            return _dbContext.CreditApplications.Where(o => o.UserID == userID);
        }
    }

    public IQueryable<Order> Orders
    {
        get
        {
            var userID = GetUserID();
            return _dbContext.Orders.Where(o => o.JuiceShopUserID == userID);
        }
    }

    private int GetUserID()
    {
        return int.Parse(_user.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }
}
