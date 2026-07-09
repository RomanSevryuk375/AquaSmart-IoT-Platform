using MediatR;

namespace Contracts.Abstractions;

public interface IDomainEvent : INotification
{
    public DateTime OccurredOn { get; }
}
