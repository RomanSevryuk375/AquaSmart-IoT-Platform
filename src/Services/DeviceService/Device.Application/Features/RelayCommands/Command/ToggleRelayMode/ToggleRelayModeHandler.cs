using BuildingBlocks.Domain.Results;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayMode;

public sealed class ToggleRelayModeHandler(
    IRelayRepository relayRepository,
    IControllerRepository controllerRepository)
    : IRequestHandler<ToggleRelayModeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ToggleRelayModeCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);

        existingRelay!.SetMode(!existingRelay.IsManual, request.UserId);

        Controller? controller = await controllerRepository.GetByIdAsync(
            existingRelay.ControllerId, cancellationToken);
        controller?.IncrementConfigVersion("relay_mode_changed");

        return Result<bool>.Success(existingRelay.IsManual);
    }
}
