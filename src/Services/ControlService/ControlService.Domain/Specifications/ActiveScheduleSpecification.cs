using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Specifications;

public sealed class ActiveScheduleSpecification
    : BaseSpecification<Schedule>
{
    public ActiveScheduleSpecification() : base(data => data.IsEnabled)
    {
    }
}
