using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;

namespace Device.Application.Features.Controllers.Query.GetControllerConfig;

public sealed record GetControllerConfigQuery
    : IQuery<Result<ControllerConfig>>
{
    public string MacAddress { get; init; } = string.Empty;
}
