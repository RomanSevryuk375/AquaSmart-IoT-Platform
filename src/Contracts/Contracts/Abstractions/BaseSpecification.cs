using System.Linq.Expressions;

namespace Contracts.Abstractions;

public abstract class BaseSpecification<T>(Expression<Func<T, bool>> criteria)
{
    public Expression<Func<T, bool>> Criteria = criteria;
}
