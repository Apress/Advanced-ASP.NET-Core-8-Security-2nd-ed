using JuiceShopDotNet.API.Authorization;
using JuiceShopDotNet.API.Data;
using JuiceShopDotNet.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text;

namespace JuiceShopDotNet.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class VaultController : Controller
{
    private readonly DatabaseContext _dbContext;
    public VaultController(DatabaseContext databaseContext)
    { 
        _dbContext = databaseContext;
    }

    [HttpPost]
    [ValidateSignature]
    public IActionResult GetCreditApplication([FromBody]IDWrapper model)
    {
        var application = _dbContext.CreditApplications.Single(ca => ca.CreditApplicationID == model.id);

        var toReturn = new CreditApplicationModel();
        toReturn.CreditApplicationID = application.CreditApplicationID;
        toReturn.SocialSecurityNumber = application.SocialSecurityNumber;

        return Json(toReturn);
    }

    [HttpPost]
    [ValidateSignature]
    public IActionResult SaveCreditApplication([FromBody]CreditApplicationModel model)
    {
        var newApplication = _dbContext.CreditApplications.SingleOrDefault(ca => ca.CreditApplicationID == model.CreditApplicationID);

        if (newApplication == null) 
        {
            newApplication = new CreditApplication();
            newApplication.CreditApplicationID = model.CreditApplicationID;
            _dbContext.CreditApplications.Add(newApplication);
        }

        newApplication.SocialSecurityNumber = model.SocialSecurityNumber;
        _dbContext.SaveChanges();

        return Ok();
    }

    [HttpPost]
    [ValidateSignature]
    public IActionResult GetJuiceShopUser([FromBody] IDWrapper model)
    {
        var user = _dbContext.JuiceShopUsers.Single(u => u.JuiceShopUserID == model.id);

        var toReturn = new JuiceShopUserModel();
        toReturn.JuiceShopUserID = user.JuiceShopUserID;
        toReturn.UserName = user.UserName;
        toReturn.UserEmail = user.UserEmail;
        toReturn.NormalizedUserEmail = user.NormalizedUserEmail;

        return Json(toReturn);
    }

    [HttpPost]
    [ValidateSignature]
    public IActionResult SaveJuiceShopUser([FromBody] JuiceShopUserModel model)
    {
        var newUser = _dbContext.JuiceShopUsers.SingleOrDefault(u => u.JuiceShopUserID == model.JuiceShopUserID);

        if (newUser == null)
        {
            newUser = new JuiceShopUser();
            newUser.JuiceShopUserID = model.JuiceShopUserID;
            _dbContext.JuiceShopUsers.Add(newUser);
        }

        newUser.UserName = model.UserName;
        newUser.UserEmail = model.UserEmail;
        newUser.NormalizedUserEmail = model.NormalizedUserEmail;
        _dbContext.SaveChanges();

        return Ok();
    }
}
