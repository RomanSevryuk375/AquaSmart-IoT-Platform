namespace Device.Domain.Specifications;

public sealed class RelayFilterSpecification(Guid? controllerId, Guid? userId, RelayPurposeEnum? purpose,
    bool? isActive, bool? isManual)
        : BaseSpecification<Relay>(data => 
                (!controllerId.HasValue || data.ControllerId == controllerId.Value) &&
                (!userId.HasValue || data.UserId == userId.Value) &&
                (!purpose.HasValue || data.Purpose == purpose.Value) &&
                (!isActive.HasValue || data.IsActive == isActive.Value) &&
                (!isManual.HasValue || data.IsManual == isManual.Value))
{
}

