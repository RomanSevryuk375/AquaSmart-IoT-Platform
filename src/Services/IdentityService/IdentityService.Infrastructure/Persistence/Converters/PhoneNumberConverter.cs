using IdentityService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IdentityService.Infrastructure.Persistence.Converters;

internal class PhoneNumberConverter : ValueConverter<PhoneNumber, string>
{
    public PhoneNumberConverter() : base(
        vo => vo.Value,
        dbVal => PhoneNumber.Parse(dbVal))
    {
    }
}
