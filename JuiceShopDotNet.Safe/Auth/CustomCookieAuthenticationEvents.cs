using JuiceShopDotNet.Safe.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace JuiceShopDotNet.Safe.Auth;

public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    public CustomCookieAuthenticationEvents()
    {
        base.OnValidatePrincipal = context => {
            var identityOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<IdentityOptions>>();
            var stampClaim = context.Principal.Claims.SingleOrDefault(c => c.Type == identityOptions.Value.ClaimsIdentity.SecurityStampClaimType);

            if (stampClaim == null)
            {
                context.HttpContext.SignOutAsync().Wait();
            }
            else
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<JuiceShopUser>>();
                var user = userManager.GetUserAsync(context.Principal).Result;
                var stamp = userManager.GetSecurityStampAsync(user).Result;

                if (stamp != stampClaim.Value)
                {
                    context.Principal = new System.Security.Claims.ClaimsPrincipal();
                }
            }

            return Task.CompletedTask;
        };
    }
}
