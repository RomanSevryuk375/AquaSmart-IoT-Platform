using Device.Domain.DomainEvents.ControllerEvents;

namespace Device.Domain.Entities;

public sealed class Controller : AggregateRoot, IEntity
{
    private Controller(
        Guid id,
        Guid userId,
        MacAddress macAddress,
        string deviceTokenHash,
        DeviceName name,
        bool isOnline,
        DateTime lastSeenAt,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        MacAddress = macAddress;
        DeviceTokenHash = deviceTokenHash;
        Name = name;
        IsOnline = isOnline;
        LastSeenAt = lastSeenAt;
        CreatedAt = createdAt;
    }

    private readonly List<Sensor> _sensors = [];
    private readonly List<Relay> _relays = [];

    public Guid Id { get; init; }
    public Guid UserId { get; private set; }
    public MacAddress MacAddress { get; private set; }
    public string DeviceTokenHash { get; private set; } = string.Empty;
    public DeviceName Name { get; private set; }
    public bool IsOnline { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<Sensor> Sensors => _sensors.AsReadOnly();
    public IReadOnlyList<Relay> Relays => _relays.AsReadOnly();

    public static Result<Controller> Create(
        Guid id, Guid userId, string rawMacAddress, string deviceTokenHash, string rawName, bool isOnline)
    {
        var errors = new List<string>();

        Result<MacAddress> macAddressResult = MacAddress.Create(rawMacAddress);
        if (macAddressResult.IsFailure)
        {
            errors.Add(macAddressResult.Error.Message);
        }

        Result<DeviceName> nameResut = DeviceName.Create(rawName);
        if (nameResut.IsFailure)
        {
            errors.Add(nameResut.Error.Message);
        }

        if (errors.Count != 0)
        {
            return Result<Controller>.Failure(Error.Validation<Controller>(
                string.Join(", ", errors)));
        }

        var controller = new Controller(
            id,
            userId,
            macAddressResult.Value,
            deviceTokenHash.Trim(),
            nameResut.Value,
            isOnline,
            lastSeenAt: DateTime.UtcNow,
            createdAt: DateTime.UtcNow);

        return Result<Controller>.Success(controller);
    }

    public Result Update(string rawMacAddress, string rawName)
    {
        var errors = new List<string>();

        Result<MacAddress> macAddressResult = MacAddress.Create(rawMacAddress);
        if (macAddressResult.IsFailure)
        {
            errors.Add(macAddressResult.Error.Message);
        }

        Result<DeviceName> nameResut = DeviceName.Create(rawName);
        if (nameResut.IsFailure)
        {
            errors.Add(nameResut.Error.Message);
        }

        if (errors.Count != 0)
        {
            return Result<Controller>.Failure(Error.Validation<Controller>(
                string.Join(", ", errors)));
        }

        MacAddress = macAddressResult.Value;
        Name = nameResut.Value;

        return Result.Success();
    }

    public void RecordPing() => LastSeenAt = DateTime.UtcNow;

    public void ToggleState()
    {
        IsOnline = !IsOnline;

        if (IsOnline is false)
        {
            RaiseEvent(new ControllerNotOnlineDomainEvent
            {
                UserId = UserId,
                ControllerId = Id,
                LastSeenAt = LastSeenAt,
            });
        }
    }

    public void SetOffline()
    {
        IsOnline = false;

        RaiseEvent(new ControllerNotOnlineDomainEvent
        {
            UserId = UserId,
            ControllerId = Id,
            LastSeenAt = LastSeenAt,
        });
    }

    public void AddSensor(Sensor sensor) => _sensors.Add(sensor);
    public void RemoveSensor(Sensor sensor)
    {
        sensor.MarkAsDeleted();
        _sensors.Remove(sensor);
    }

    public void AddRelay(Relay relay) => _relays.Add(relay);
    public void RemoveRelay(Relay relay)
    {
        relay.MarkAsDeleted();
        _relays.Remove(relay);
    }
}
