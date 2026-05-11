namespace Device.Domain.SpecificationParams;

public sealed record ControllerFilterParams
{
    public Guid? UserId { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsOnline { get; init; }
}
