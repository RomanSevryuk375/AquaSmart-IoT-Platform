using Contracts.Constants;
using Device.Application.DTOs.Sensor;
using FluentValidation;

namespace Device.Application.DTOs.Validators.SensorValidators;

public class SensorUpdateRequestDtoValidator
    : AbstractValidator<SensorUpdateRequestDto>
{
    public SensorUpdateRequestDtoValidator()
    {
        RuleFor(x => x.ControllerId)
            .NotEmpty();

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
