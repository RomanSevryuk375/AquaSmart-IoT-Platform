using BuildingBlocks.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Billing.Infrastructure.Persistence.Converters;

internal sealed class EmailAddressConverter : ValueConverter<EmailAddress, string>
{
    public EmailAddressConverter() : base(
        vo => vo.Value,
        dbVal => EmailAddress.Parse(dbVal))
    {
    }
}
