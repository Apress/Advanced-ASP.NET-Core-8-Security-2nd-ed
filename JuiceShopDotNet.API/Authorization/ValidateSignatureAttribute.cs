using JuiceShopDotNet.Common.Cryptography.AsymmetricEncryption;
using JuiceShopDotNet.Common.Cryptography.KeyStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JuiceShopDotNet.API.Authorization;

public class ValidateSignatureAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string body = "";

        context.HttpContext.Request.EnableBuffering();
        context.HttpContext.Request.Body.Position = 0;

        body = new StreamReader(context.HttpContext.Request.Body).ReadToEndAsync().Result;
        context.HttpContext.Request.Body.Position = 0;

        var signatureService = context.HttpContext.RequestServices.GetRequiredService<ISignatureService>();

        var timeStamp = context.HttpContext.Request.Headers["Timestamp"].Single();
        DateTime timeStampAsDate;

        if (!DateTime.TryParse(timeStamp, out timeStampAsDate))
        {
            context.Result = new UnauthorizedObjectResult("Unauthorized"); //Just say "unauthorized" to avoid giving attackers too many clues as to why the request failed
            return;
        }

        //Check if the timestamp is reasonably close to now to prevent replay attacks
        if (timeStampAsDate.AddMinutes(2) < DateTime.UtcNow ||
            timeStampAsDate.AddMinutes(-2) > DateTime.UtcNow)
        {
            context.Result = new UnauthorizedObjectResult("Unauthorized");
            return;
        }

        var signatureContent = $"{timeStamp}|{body}";
        var signature = context.HttpContext.Request.Headers["Signature"].Single();

        if (!signatureService.VerifySignature(signatureContent, signature, "API_PUBLIC_KEY"))
        {
            context.Result = new UnauthorizedObjectResult("Unauthorized");
            return;
        }
    }
}
