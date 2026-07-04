using Contracts.Abstractions;
using Contracts.Results;
using Control.Domain.ValueObjects;

namespace Control.Domain.Entities;

public sealed class VacationMode : AggregateRoot, IEntity
{
    private VacationMode(
        Guid id,
        Guid ecosystemId,
        DateRange dateRange,
        bool isActive,
        double calculatedFeed,
        DateTime createdAt)
    {
        Id = id;
        EcosystemId = ecosystemId;
        DateRange = dateRange;
        IsActive = isActive;
        CalculatedFeed = calculatedFeed;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private VacationMode() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid EcosystemId { get; private set; }
    public DateRange DateRange { get; private set; }
    public bool IsActive { get; private set; }
    public double CalculatedFeed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<VacationMode> Create(
        Guid vacationModeId,
        Guid ecosystemId,
        DateTime startDate,
        DateTime endDate,
        bool isActive,
        double calculatedFeed)
    {
        Result<DateRange> dateRangeResult = DateRange.Create(startDate, endDate);
        if (dateRangeResult.IsFailure)
        {
            return Result<VacationMode>.Failure(dateRangeResult.Error);
        }

        var vacation = new VacationMode(
            vacationModeId, ecosystemId,
            dateRangeResult.Value, isActive, calculatedFeed,
            createdAt: DateTime.UtcNow);

        return Result<VacationMode>.Success(vacation);
    }

    public void SetActive(bool status)
    {
        if (IsActive == status)
        {
            return;
        }

        IsActive = status;

        IncrementVersion();
    }

    public Result SetTiming(DateTime startDate, DateTime endDate)
    {
        Result<DateRange> dateRangeResult = DateRange.Create(startDate, endDate);
        if (dateRangeResult.IsFailure)
        {
            return Result<VacationMode>.Failure(dateRangeResult.Error);
        }

        DateRange = dateRangeResult.Value;

        IncrementVersion();

        return Result.Success();
    }

    public Result SetFeedSize(double calculatedFeed)
    {
        if (calculatedFeed < 0)
        {
            return Result.Failure(Error.Validation<VacationMode>(
                "CalculatedFeed cannot be negative."));
        }

        CalculatedFeed = calculatedFeed;

        IncrementVersion();

        return Result.Success();
    }
}
