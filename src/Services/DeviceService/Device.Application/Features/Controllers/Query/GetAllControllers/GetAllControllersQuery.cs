using Contracts.Abstractions;
using Device.Application.Features.Controllers.Query.Shared;

namespace Device.Application.Features.Controllers.Query.GetAllControllers;

public sealed record GetAllControllersQuery
    : IQuery<Result<IReadOnlyList<ControllerDto>>>
{
    public Guid UserId { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsOnline { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
