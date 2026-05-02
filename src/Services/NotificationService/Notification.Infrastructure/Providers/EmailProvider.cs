using Contracts.Options;
using Microsoft.Extensions.Options;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Notification.Infrastructure.Providers;

public class EmailProvider(IOptions<EmailOptions> options) : INotificationProvider
{
    private readonly EmailOptions _settings = options.Value;
    public bool IsEnabled(UserEntity user)
    {
        if (user.EmailEnable)
        {
            return true;
        }

        return false;
    }

    public async Task<(bool Success, string Error)> SendAsync(
        UserEntity user, 
        string message, 
        CancellationToken cancellationToken)
    {
        try
        {
            var email = user.Email;

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(_settings.FromEmail, user.Email)
            {
                Subject = "AquaAPI Notification",
                Body = message
            };

            await client.SendMailAsync(mailMessage, cancellationToken);
            return (true, "Ok");
        }
        catch (Exception ex)
        {
            return (false, $"{ex.Message} {ex.StackTrace}");
        }
    }
}
