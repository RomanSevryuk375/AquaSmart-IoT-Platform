namespace Device.TestShared.Builders;

public class RelayCommandBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _controllerId = TestConstants.ControllerId;
    private Guid _relayId = TestConstants.RelayId;
    private bool _targetState = true;
    private DateTime? _expireAt = null;

    public RelayCommandBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RelayCommandBuilder WithControllerId(Guid controllerId)
    {
        _controllerId = controllerId;
        return this;
    }

    public RelayCommandBuilder WithRelayId(Guid relayId)
    {
        _relayId = relayId;
        return this;
    }

    public RelayCommandBuilder WithTargetState(bool targetState)
    {
        _targetState = targetState;
        return this;
    }

    public RelayCommandBuilder WithExpireAt(DateTime? expireAt)
    {
        _expireAt = expireAt;
        return this;
    }

    public RelayCommand Build()
    {
        Result<RelayCommand> result = RelayCommand.Create(
            _id, _controllerId, _relayId, _targetState, _expireAt);
        if (result.IsFailure)
        {
            throw new ArgumentException($"RelayCommandBuilder failed: {result.Error.Message}");
        }

        return result.Value;
    }
}
