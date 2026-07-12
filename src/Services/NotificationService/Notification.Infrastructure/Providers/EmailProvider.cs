using System.Net;
using System.Net.Mail;
using Contracts.Constants;
using Contracts.Options;
using Contracts.Results;
using Microsoft.Extensions.Options;
using Notification.Domain.Interfaces;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Providers;

public sealed class EmailProvider(IOptions<EmailOptions> options) : IEmailProvider
{
    private readonly EmailOptions _settings = options.Value;

    public async Task<Result> SendAsync(
        NotificationRecipient recipient,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipient.Email))
        {
            return Result.Failure(Error.Validation(
                ErrorCodes.NotificationProvider.EmailMissing,
                ErrorMessages.NotificationProvider.EmailMissing));
        }

        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage(_settings.FromEmail, recipient.Email)
            {
                Subject = "AquaAPI Notification",
                Body = message
            };

            await client.SendMailAsync(mailMessage, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure(
                ErrorCodes.NotificationProvider.EmailProviderError,
                ex.Message));
        }
    }
}
