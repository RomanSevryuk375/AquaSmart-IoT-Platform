using Contracts.Results;
using Device.Domain.Entities;
using Device.TestShared.Constants;

namespace Device.TestShared.Builders;

public class ControllerBuilder
{
    private Guid _id = TestConstants.ControllerId;
    private Guid _userId = TestConstants.UserId;
    private readonly string _macAddress = TestConstants.ValidMacAddress;
    private readonly string _deviceTokenHash = TestConstants.ValidTokenHash;
    private readonly string _name = TestConstants.ValidDeviceName;
    private bool _isOnline = true;

    public ControllerBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ControllerBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public ControllerBuilder AsOffline()
    {
        _isOnline = false;
        return this;
    }

    public Controller Build()
    {
        Result<Controller> result = Controller.Create(
            _id, _userId, _macAddress, _deviceTokenHash, _name, _isOnline);

        if (result.IsFailure)
        {
            throw new ArgumentException($"Builder failed: {result.Error.Message}");
        }

        Controller controller = result.Value;
        controller.ClearDomainEvents();
        return controller;
    }
}
