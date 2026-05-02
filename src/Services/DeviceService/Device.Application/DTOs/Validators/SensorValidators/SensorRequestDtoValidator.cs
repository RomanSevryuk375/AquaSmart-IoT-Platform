using Contracts.Constants;
using Device.Application.DTOs.Sensor;
using FluentValidation;

namespace Device.Application.DTOs.Validators.SensorValidators;

public sealed class SensorRequestDtoValidator
    : AbstractValidator<SensorRequestDto>
{
    public SensorRequestDtoValidator()
    {
        RuleFor(x => x.ControllerId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(SensorConstants.NameLength);

        RuleFor(x => x.ConnectionProtocol)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.ConnectionAddress)
            .NotEmpty()
            .MaximumLength(SensorConstants.ConnectionAddressLength);

        RuleFor(x => x.Type)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.Unit)
            .NotEmpty()
            .MaximumLength(SensorConstants.UnitLength);
    }
}
