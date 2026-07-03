using Contracts.Results;

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
                "StartDate must be strictly before EndDate."));
        }

        return Result<DateRange>.Success(new DateRange(start, end));
    }

    public static DateRange Parse(string dbValue)
    {
        string[] parts = dbValue.Split("_", 2);
        var start = Convert.ToDateTime(parts[0]);
        var end = Convert.ToDateTime(parts[1]);

        return Create(start, end).Value;
    }

    public override string ToString() => $"{StartDate}_{EndDate}";
}
