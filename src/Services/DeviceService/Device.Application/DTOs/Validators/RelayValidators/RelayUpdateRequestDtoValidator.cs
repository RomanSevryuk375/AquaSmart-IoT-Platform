using Contracts.Constants;

namespace Device.Application.DTOs.Validators.RelayValidators;

public sealed class RelayUpdateRequestDtoValidator
    : AbstractValidator<RelayUpdateRequestDto>
{
    public RelayUpdateRequestDtoValidator()
    {
        RuleFor(x => x.ConnectionAddress)
            .NotEmpty();

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
    }
}
