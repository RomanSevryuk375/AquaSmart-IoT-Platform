using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace IdentityService.Domain.ValueObjects;

public sealed record TimeZoneId
{
    public string Value { get; }

    internal TimeZoneId(string value)
    {
        Value = value;
    }

    public static TimeZoneId Parse(string dbVal) => new(dbVal);

    public static Result<TimeZoneId> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<TimeZoneId>.Failure(Error.Validation<TimeZoneId>(
                ErrorMessages.Identity.TimeZoneRequired));
        }

        string cleanZone = value.Trim();

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(cleanZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return Result<TimeZoneId>.Failure(Error.Validation<TimeZoneId>(
                string.Format(ErrorMessages.Identity.TimeZoneNotRecognized, cleanZone)));
        }

        return Result<TimeZoneId>.Success(new TimeZoneId(cleanZone));
    }

    public override string ToString() => Value;
}
