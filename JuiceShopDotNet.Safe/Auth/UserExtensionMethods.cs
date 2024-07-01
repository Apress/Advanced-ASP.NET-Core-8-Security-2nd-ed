using System.Security.Claims;

namespace JuiceShopDotNet.Safe.Auth;

public static class UserExtensionMethods
{
    public static int GetUserID(this ClaimsPrincipal principal)
    { 
        var idAsString = principal.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
        return int.Parse(idAsString);
    }
}
