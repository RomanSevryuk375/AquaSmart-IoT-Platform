using System.Globalization;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;

namespace Control.Domain.ValueObjects;

public sealed record DateRange
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private DateRange(DateTime start, DateTime end)
    {
        StartDate = start;
        EndDate = end;
    }

    public static Result<DateRange> Create(DateTime start, DateTime end)
    {
        if (start >= end)
        {
            return Result<DateRange>.Failure(Error.Validation<DateRange>(
                ControlValidationMessages.StartDateMustBeBeforeEndDate));
        }

        return Result<DateRange>.Success(new DateRange(start, end));
    }

    public static DateRange Parse(string dbValue)
    {
        string[] parts = dbValue.Split("_", 2);
        var start = Convert.ToDateTime(parts[0], CultureInfo.InvariantCulture);
        var end = Convert.ToDateTime(parts[1], CultureInfo.InvariantCulture);

        return Create(start, end).Value;
    }

    public override string ToString() => $"{StartDate:O}_{EndDate:O}";
}
