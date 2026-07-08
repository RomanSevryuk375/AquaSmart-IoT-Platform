using System.Net;
using System.Net.Mail;
using Contracts.Options;
using Contracts.Results;
using Microsoft.Extensions.Options;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Providers;

public sealed class EmailProvider(IOptions<EmailOptions> options) : INotificationProvider
{
    private readonly EmailOptions _settings = options.Value;

    public bool IsEnabled(User user) => user.EmailEnable;

    public async Task<Result> SendAsync(
        User user,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage(_settings.FromEmail, user.Email.Value)
            {
                Subject = "AquaAPI Notification",
                Body = message
            };

            await client.SendMailAsync(mailMessage, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure<EmailProvider>(
                $"{ex.Message} {ex.StackTrace}"));
        }
    }
}
