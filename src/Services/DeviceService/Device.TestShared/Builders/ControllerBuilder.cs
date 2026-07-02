namespace Device.TestShared.Builders;

public class ControllerBuilder
{
    private Guid _id = TestConstants.ControllerId;
    private Guid _userId = TestConstants.UserId;
    private string _macAddress = TestConstants.ValidMacAddress;
    private string _deviceTokenHash = TestConstants.ValidTokenHash;
    private string _name = TestConstants.ValidDeviceName;
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

    public ControllerBuilder WithMacAddress(string macAddress)
    {
        _macAddress = macAddress;
        return this;
    }

    public ControllerBuilder WithDeviceTokenHash(string hash)
    {
        _deviceTokenHash = hash;
        return this;
    }

    public ControllerBuilder WithName(string name)
    {
        _name = name;
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
            _id,
            _userId,
            _macAddress,
            _deviceTokenHash,
            _name,
            _isOnline);

        if (result.IsFailure)
        {
            throw new ArgumentException($"ControllerBuilder failed: {result.Error.Message}");
        }

        Controller controller = result.Value;
        controller.ClearDomainEvents();

        return controller;
    }
}
