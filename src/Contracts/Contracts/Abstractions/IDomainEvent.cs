using MediatR;

namespace Contracts.Abstractions;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
