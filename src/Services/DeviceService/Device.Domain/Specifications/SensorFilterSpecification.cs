namespace Device.Domain.Specifications;

public sealed class SensorFilterSpecification(Guid? controllerId, Guid? userId, SensorType? type, SensorState? state)
        : BaseSpecification<Sensor>(data =>
            (!controllerId.HasValue || data.ControllerId == controllerId.Value) &&
            (!userId.HasValue || data.UserId == userId.Value) &&
            (!type.HasValue || data.Type == type.Value) &&
            (!state.HasValue || data.State == state.Value))
{
}
