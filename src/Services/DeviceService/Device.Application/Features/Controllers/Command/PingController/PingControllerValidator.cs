namespace Device.Application.Features.Controllers.Command.PingController;

internal sealed class PingControllerValidator
    : AbstractValidator<PingControllerCommand>
{
    public PingControllerValidator()
    {
        RuleFor(x => x.ControllerId)
            .NotEmpty();

        RuleFor(x => x.DeviceToken)
            .NotEmpty();
    }
}
