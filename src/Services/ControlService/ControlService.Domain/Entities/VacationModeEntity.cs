using Contracts.Abstractions;
using Contracts.Results;

namespace Control.Domain.Entities;

public sealed class VacationModeEntity : IEntity
{
    private VacationModeEntity(
        Guid id,
        Guid ecosystemId,
        DateTime startDate,
        DateTime endDate,
        bool isActive,
        double calculatedFeed,
        DateTime createdAt)
    {
        Id = id;
        EcosystemId = ecosystemId;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        CalculatedFeed = calculatedFeed;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid EcosystemId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public double CalculatedFeed { get; private set; }
    public DateTime CreatedAt { get; private set; }

#pragma warning disable CS8618
    private VacationModeEntity() { }
#pragma warning restore CS8618

    public static Result<VacationModeEntity> Create(
       Guid aquariumId,
       DateTime startDate,
       DateTime endDate,
       bool isActive,
       double calculatedFeed)
    {
        var errors = new List<string>();

        if (aquariumId == Guid.Empty)
        {
            errors.Add("AquariumId must not be empty.");
        }

        if (endDate <= startDate)
        {
            errors.Add("EndDate must be later than StartDate.");
        }

        if (calculatedFeed < 0)
        {
            errors.Add("CalculatedFeed cannot be negative.");
        }

        if (errors.Count > 0)
        {
            return Result<VacationModeEntity>.Failure(
                Error.Validation(
                    "VacationMode.Invalid",
                    string.Join("; ", errors)));
        }

        var vacation = new VacationModeEntity(
            Guid.NewGuid(),
            aquariumId,
            startDate,
            endDate,
            isActive,
            calculatedFeed,
            DateTime.UtcNow);

        return Result<VacationModeEntity>.Success(vacation);
    }

    public Result Update(
        DateTime startDate,
        DateTime endDate,
        bool isActive,
        double calculatedFeed)
    {
        var errors = new List<string>();

        if (endDate <= startDate)
        {
            errors.Add("EndDate must be later than StartDate.");
        }

        if (calculatedFeed < 0)
        {
            errors.Add("CalculatedFeed cannot be negative.");
        }

        if (errors.Count > 0)
        {
            return Result.Failure(Error.Validation(
                "VacationMode.Invalid",
                string.Join("; ", errors)));
        }

        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        CalculatedFeed = calculatedFeed;

        return Result.Success();
    }

    public void SetActive(bool status) => IsActive = status;

    public Result SetTiming(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
        {
            return Result.Failure(Error.Validation(
                "VacationMode.Invalid",
                "EndDate must be later than StartDate."));
        }

        StartDate = startDate;
        EndDate = endDate;

        return Result.Success();
    }

    public Result SetFeedSize(double calculatedFeed)
    {
        if (calculatedFeed < 0)
        {
            return Result.Failure(Error.Validation(
                "VacationMode.Invalid",
                "CalculatedFeed cannot be negative."));
        }

        CalculatedFeed = calculatedFeed;

        return Result.Success();
    }
}
