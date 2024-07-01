using Microsoft.AspNetCore.Antiforgery;
using System.Text;

namespace JuiceShopDotNet.Safe.CSRF;

public class AntiforgeryAdditionalDataProvider : IAntiforgeryAdditionalDataProvider
{
    private const int EXPIRATION_MINUTES = 60;
    public string GetAdditionalData(HttpContext context)
    {
        return string.Format("Expiration={0}", DateTime.UtcNow.AddMinutes(EXPIRATION_MINUTES));
    }

    public bool ValidateAdditionalData(HttpContext context, string additionalData)
    {
        try
        {
            bool isValid = true;

            if (!additionalData.StartsWith("Expiration="))
                isValid = false;

            if (!additionalData.Contains("="))
                //We'll get errors below, so just return false here
                return false;

            string value = additionalData.Substring(additionalData.IndexOf("=") + 1);

            var expiration = DateTime.Parse(value);
            if (DateTime.UtcNow > expiration)
                isValid = false;

            return isValid;
        }
        catch (Exception ex)
        {
            //TODO: Log this
            return false;
        }
    }
}
