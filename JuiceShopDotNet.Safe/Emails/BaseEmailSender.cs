using System.Text;

namespace JuiceShopDotNet.Safe.Emails;

public abstract class BaseEmailSender
{
    public void SendEmail(string email, string subject, string htmlMessage)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Email: " + email);
        sb.AppendLine("Subject: " + subject);
        sb.AppendLine("Message: " + htmlMessage);

        System.IO.File.WriteAllText("C:\\Emails\\" + Guid.NewGuid().ToString() + ".txt", sb.ToString());
    }
}
