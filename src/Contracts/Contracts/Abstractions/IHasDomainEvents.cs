
namespace Contracts.Abstractions;

public interface IHasDomainEvents
{
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    public void ClearDomainEvents();
}
