using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Device.Domain.Events.ControllerEvents;

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
        ConfigVersion = 1;
        OfflineAlertSent = false;
    }

#pragma warning disable CS8618
    private Controller() { }
#pragma warning restore CS8618

    public Guid Id { get; init; }
    public Guid UserId { get; private set; }
    public MacAddress MacAddress { get; private set; }
    public string DeviceTokenHash { get; private set; }
    public DeviceName Name { get; private set; }
    public bool IsOnline { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public long ConfigVersion { get; private set; } = 1;
    public string? FirmwareVersion { get; private set; }
    public string? IpAddress { get; private set; }
    public int? Rssi { get; private set; }
    public long? UptimeSec { get; private set; }
    public bool OfflineAlertSent { get; private set; }

    public static Result<Controller> Create(
        Guid id, Guid userId, string rawMacAddress, string deviceTokenHash, string rawName, bool isOnline)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "Controller Id", errors);
        if (userId == Guid.Empty)
        {
            errors.Add(CommonErrors.IdEmpty);
        }

        MacAddress? macAddress = DomainGuard.Collect(MacAddress.Create(rawMacAddress), errors);
        DeviceName? name = DomainGuard.Collect(DeviceName.Create(rawName), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Controller>(errors);
        }

        var controller = new Controller(
            id,
            userId,
            macAddress!,
            deviceTokenHash.Trim(),
            name!,
            isOnline,
            lastSeenAt: DateTime.UtcNow,
            createdAt: DateTime.UtcNow);

        return Result<Controller>.Success(controller);
    }

    public Result Update(string rawMacAddress, string rawName)
    {
        var errors = new List<string>();

        MacAddress? macAddress = DomainGuard.Collect(MacAddress.Create(rawMacAddress), errors);
        DeviceName? name = DomainGuard.Collect(DeviceName.Create(rawName), errors);

        if (errors.Count != 0)
        {
            return Result.Failure(Error.Validation<Controller>(string.Join(", ", errors)));
        }

        MacAddress = macAddress!;
        Name = name!;

        IncrementVersion();

        return Result.Success();
    }

    public void RecordPing()
    {
        LastSeenAt = DateTime.UtcNow;
        OfflineAlertSent = false;
        if (!IsOnline)
        {
            IsOnline = true;
        }

        IncrementVersion();
    }

    public void RecordMqttOnline(
        string? firmwareVersion,
        string? ipAddress,
        int? rssi,
        long? uptimeSec)
    {
        IsOnline = true;
        LastSeenAt = DateTime.UtcNow;
        OfflineAlertSent = false;

        if (!string.IsNullOrWhiteSpace(firmwareVersion))
        {
            FirmwareVersion = firmwareVersion.Trim();
        }

        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            IpAddress = ipAddress.Trim();
        }

        if (rssi.HasValue)
        {
            Rssi = rssi;
        }

        if (uptimeSec.HasValue && uptimeSec.Value >= 0)
        {
            UptimeSec = uptimeSec;
        }

        IncrementVersion();
    }

    public void MarkOfflineFromLwt()
    {
        if (!IsOnline)
        {
            return;
        }

        IsOnline = false;
        IncrementVersion();
    }

    public void MarkOfflineAlertSent()
    {
        if (OfflineAlertSent)
        {
            return;
        }

        OfflineAlertSent = true;

        RaiseEvent(new ControllerNotOnlineDomainEvent
        {
            UserId = UserId,
            ControllerId = Id,
            LastSeenAt = LastSeenAt,
        });

        IncrementVersion();
    }

    public void IncrementConfigVersion(string reason = "relay_or_sensor_updated")
    {
        ConfigVersion++;

        RaiseEvent(new ControllerConfigChangedDomainEvent
        {
            ControllerId = Id,
            MacAddress = MacAddress.Value,
            ConfigVersion = ConfigVersion,
            Reason = string.IsNullOrWhiteSpace(reason) ? "relay_or_sensor_updated" : reason.Trim(),
        });

        IncrementVersion();
    }

    public void ToggleState()
    {
        IsOnline = !IsOnline;

        if (!IsOnline)
        {
            OfflineAlertSent = true;
            RaiseEvent(new ControllerNotOnlineDomainEvent
            {
                UserId = UserId,
                ControllerId = Id,
                LastSeenAt = LastSeenAt,
            });
        }
        else
        {
            OfflineAlertSent = false;
        }

        IncrementVersion();
    }

    public void SetOffline()
    {
        IsOnline = false;
        OfflineAlertSent = true;

        RaiseEvent(new ControllerNotOnlineDomainEvent
        {
            UserId = UserId,
            ControllerId = Id,
            LastSeenAt = LastSeenAt,
        });

        IncrementVersion();
    }
}
