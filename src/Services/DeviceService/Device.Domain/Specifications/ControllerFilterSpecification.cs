namespace Device.Domain.Specifications;

public sealed class ControllerFilterSpecification(Guid? userId, string? searchTerm, bool? isOnline)
        : BaseSpecification<Controller>(data => 
            (!isOnline.HasValue || data.IsOnline == isOnline.Value) 
            && (!userId.HasValue || data.UserId == userId.Value)
            && (string.IsNullOrWhiteSpace(searchTerm)
                || data.Name.Value.ToLower().Contains(searchTerm.ToLower())
                || data.MacAddress.Value.ToLower().Contains(searchTerm.ToLower())))
{
}
