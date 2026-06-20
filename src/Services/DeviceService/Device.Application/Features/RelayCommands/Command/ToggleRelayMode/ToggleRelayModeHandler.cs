namespace Device.Application.Features.RelayCommands.Command.ToggleRelayMode;

internal sealed class ToggleRelayModeHandler(
    IRelayRepository relayRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleRelayModeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ToggleRelayModeCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);

        existingRelay!.SetMode(!existingRelay.IsManual);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(existingRelay.IsManual);
    }
}
