using Control.TestShared.Constants;

namespace Control.TestShared.Builders;

public class VacationModeBuilder
{
    private Guid _id = ControlTestConstants.VacationModeId;
    private Guid _ecosystemId = ControlTestConstants.EcosystemId;
    private DateTime _startDate = DateTime.UtcNow.AddDays(1);
    private DateTime _endDate = DateTime.UtcNow.AddDays(10);
    private bool _isActive = false;
    private double _calculatedFeed = 50.0;

    public VacationModeBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public VacationModeBuilder WithEcosystemId(Guid ecosystemId)
    {
        _ecosystemId = ecosystemId;
        return this;
    }

    public VacationModeBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public VacationModeBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public VacationModeBuilder WithIsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public VacationModeBuilder WithCalculatedFeed(double calculatedFeed)
    {
        _calculatedFeed = calculatedFeed;
        return this;
    }

    public VacationMode Build()
    {
        Result<VacationMode> result = VacationMode.Create(
            _id,
            _ecosystemId,
            _startDate,
            _endDate,
            _isActive,
            _calculatedFeed);

        if (result.IsFailure)
        {
            throw new ArgumentException($"VacationModeBuilder failed: {result.Error.Message}");
        }

        VacationMode vacationMode = result.Value;
        vacationMode.ClearDomainEvents();
        return vacationMode;
    }
}
