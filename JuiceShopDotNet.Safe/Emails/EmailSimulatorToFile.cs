using JuiceShopDotNet.Safe.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text;

namespace JuiceShopDotNet.Safe.Emails;

public class EmailSimulatorToFile : BaseEmailSender, IEmailSender<JuiceShopUser>, IEmailSender
{
    public Task SendConfirmationLinkAsync(JuiceShopUser user, string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        base.SendEmail(email, subject, htmlMessage);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(JuiceShopUser user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(JuiceShopUser user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }
}
