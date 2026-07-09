using Contracts.Constants;
using Contracts.Results;

namespace Notification.Domain.ValueObjects;

public sealed record MessageText
{
    public string Value { get; }

    private MessageText(string value)
    {
        Value = value;
    }

    public static Result<MessageText> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<MessageText>.Failure(Error.Validation<MessageText>(
                "Notification message cannot be empty."));
        }

        string cleanMessage = value.Trim();

        if (cleanMessage.Length > NotificationConstants.MessageLength)
        {
            return Result<MessageText>.Failure(Error.Validation<MessageText>(
                $"Message cannot exceed {NotificationConstants.MessageLength} characters."));
        }

        return Result<MessageText>.Success(new MessageText(cleanMessage));
    }

    public override string ToString() => Value;
}
