using Contracts.Abstractions;

namespace Control.Domain.Entities;

public class VacationModeEntity : IEntity
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

    public static (VacationModeEntity? vacation, List<string> errors) Create(
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
            return (null, errors);
        }

        var vacation = new VacationModeEntity(
            Guid.NewGuid(),
            aquariumId,
            startDate,
            endDate,
            isActive,
            calculatedFeed,
            DateTime.UtcNow);

        return (vacation, errors);
    }

    public List<string>? Update(
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
            return errors;
        }

        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        CalculatedFeed = calculatedFeed;

        return null;
    }

    public void SetActive(bool status)
    {
        IsActive = status;
    }

    public string? SetTiming(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
        {
            return("EndDate must be later than StartDate.");
        }

        StartDate = startDate;
        EndDate = endDate;

        return null;
    }

    public string? SetFeedSize(double calculatedFeed)
    {
        if (calculatedFeed < 0)
        {
            return("CalculatedFeed cannot be negative.");
        }

        CalculatedFeed = calculatedFeed;

        return null;
    }
}
