using Contracts.Constants;
using Device.Application.DTOs.Relay;
using FluentValidation;

namespace Device.Application.DTOs.Validators.RelayValidators;

public sealed class RelayRequestDtoValidator
    : AbstractValidator<RelayRequestDto>
{
    public RelayRequestDtoValidator()
    {
        RuleFor(x => x.ConnectionAddress)
            .NotEmpty();

        RuleFor(x => x.PowerSensorId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(RelayConstants.NameLength);

        RuleFor(x => x.ConnectionProtocol)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.ConnectionAddress)
            .NotEmpty()
            .MaximumLength(RelayConstants.ConnectionAddressLength);

        RuleFor(x => x.IsNormalyOpen)
            .NotEmpty();

        RuleFor(x => x.Purpose)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.IsActive)
            .NotEmpty();

        RuleFor(x => x.IsManual)
            .NotEmpty();
    }
}
