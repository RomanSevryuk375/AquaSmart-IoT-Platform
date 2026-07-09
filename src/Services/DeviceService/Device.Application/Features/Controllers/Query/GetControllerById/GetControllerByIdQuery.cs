using Contracts.Abstractions;
using Device.Application.Features.Controllers.Query.Shared;

namespace Device.Application.Features.Controllers.Query.GetControllerById;

public sealed record GetControllerByIdQuery
    : IQuery<Result<ControllerDto>>
{
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
}
