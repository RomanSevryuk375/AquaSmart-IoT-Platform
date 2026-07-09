using Control.Domain.Interfaces;
using Control.TestShared.Constants;
using NSubstitute;

namespace Control.TestShared.Builders;

public class ScheduleBuilder
{
    private Guid _id = ControlTestConstants.ScheduleId;
    private Guid _ecosystemId = ControlTestConstants.EcosystemId;
    private Guid _relayId = ControlTestConstants.RelayId;
    private string _cronExpression = "0 12 * * *";
    private ICronValidator? _cronValidator;
    private double _durationMin = 30.0;
    private bool _isFadeMode = false;
    private bool _isEnabled = true;

    public ScheduleBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ScheduleBuilder WithEcosystemId(Guid ecosystemId)
    {
        _ecosystemId = ecosystemId;
        return this;
    }

    public ScheduleBuilder WithRelayId(Guid relayId)
    {
        _relayId = relayId;
        return this;
    }

    public ScheduleBuilder WithCronExpression(string cronExpression)
    {
        _cronExpression = cronExpression;
        return this;
    }

    public ScheduleBuilder WithCronValidator(ICronValidator cronValidator)
    {
        _cronValidator = cronValidator;
        return this;
    }

    public ScheduleBuilder WithDurationMin(double durationMin)
    {
        _durationMin = durationMin;
        return this;
    }

    public ScheduleBuilder WithIsFadeMode(bool isFadeMode)
    {
        _isFadeMode = isFadeMode;
        return this;
    }

    public ScheduleBuilder WithIsEnabled(bool isEnabled)
    {
        _isEnabled = isEnabled;
        return this;
    }

    public Schedule Build()
    {
        ICronValidator validator = _cronValidator ?? Substitute.For<ICronValidator>();
        if (_cronValidator == null)
        {
            validator.IsValid(Arg.Any<string>()).Returns(true);
        }

        Result<Schedule> result = Schedule.Create(
            _id,
            _ecosystemId,
            _relayId,
            _cronExpression,
            validator,
            _durationMin,
            _isFadeMode,
            _isEnabled);

        if (result.IsFailure)
        {
            throw new ArgumentException($"ScheduleBuilder failed: {result.Error.Message}");
        }

        Schedule schedule = result.Value;
        schedule.ClearDomainEvents();
        return schedule;
    }
}
