using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;

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
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(vacationModeId, "VacationMode Id", errors);
        DomainGuard.CheckNotEmpty(ecosystemId, nameof(EcosystemId), errors);
        DateRange? dateRange = DomainGuard.Collect(DateRange.Create(startDate, endDate), errors);
        if (calculatedFeed < ControlConstants.MinCalculatedFeed)
        {
            errors.Add(ControlValidationMessages.CalculatedFeedCannotBeNegative);
        }

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<VacationMode>(errors);
        }

        var vacation = new VacationMode(
            vacationModeId, ecosystemId,
            dateRange!, isActive, calculatedFeed,
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
        if (calculatedFeed < ControlConstants.MinCalculatedFeed)
        {
            return Result.Failure(Error.Validation<VacationMode>(
                ControlValidationMessages.CalculatedFeedCannotBeNegative));
        }

        CalculatedFeed = calculatedFeed;

        IncrementVersion();

        return Result.Success();
    }
}
