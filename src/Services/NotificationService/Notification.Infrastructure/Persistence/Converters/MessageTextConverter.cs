using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Persistence.Converters;

internal class MessageTextConverter : ValueConverter<MessageText, string>
{
    public MessageTextConverter() : base(
        vo => vo.Value,
        dbVal => MessageText.Parse(dbVal))
    {
    }
}
