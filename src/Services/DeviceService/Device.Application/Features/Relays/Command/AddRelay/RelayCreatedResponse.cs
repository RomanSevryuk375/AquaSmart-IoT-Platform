namespace Device.Application.Features.Relays.Command.AddRelay;

public sealed record RelayCreatedResponse
{
    public Guid Id { get; init; }
    public Guid ControllerId { get; init; }
    public Guid? PowerSensorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ConnectionAddress { get; init; } = string.Empty;
    public bool IsNormalyOpen { get; init; }
    public RelayPurpose Purpose { get; init; }
    public bool IsActive { get; init; }
    public bool IsManual { get; init; }
    public DateTime CreatedAt { get; init; }
}
