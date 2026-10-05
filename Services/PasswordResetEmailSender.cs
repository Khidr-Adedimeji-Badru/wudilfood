using System.Net;
using System.Net.Mail;

namespace WudilFood.Services;

public interface IPasswordResetEmailSender
{
    Task<bool> SendAsync(string recipient, string resetUrl);
}

public class PasswordResetEmailSender(IConfiguration configuration, ILogger<PasswordResetEmailSender> logger) : IPasswordResetEmailSender
{
    public async Task<bool> SendAsync(string recipient, string resetUrl)
    {
        var host=configuration["Email:SmtpHost"];
        var from=configuration["Email:FromAddress"];
        if(string.IsNullOrWhiteSpace(host)||string.IsNullOrWhiteSpace(from))
        {
            logger.LogWarning("Password reset email was not sent because SMTP is not configured.");
            return false;
        }

        using var message=new MailMessage(from,recipient)
        {
            Subject="Reset your Wudil Food password",
            Body=$"<p>Use the link below to reset your Wudil Food password.</p><p><a href=\"{WebUtility.HtmlEncode(resetUrl)}\">Reset password</a></p><p>If you did not request this, you can ignore this email.</p>",
            IsBodyHtml=true
        };
        using var client=new SmtpClient(host,configuration.GetValue("Email:SmtpPort",587))
        {
            EnableSsl=configuration.GetValue("Email:EnableSsl",true)
        };
        var username=configuration["Email:Username"];
        var password=configuration["Email:Password"];
        if(!string.IsNullOrWhiteSpace(username))client.Credentials=new NetworkCredential(username,password);

        try
        {
            await client.SendMailAsync(message);
            return true;
        }
        catch(Exception ex)
        {
            logger.LogError(ex,"Failed to send a password reset email.");
            return false;
        }
    }
}
