using System.Linq.Expressions;

namespace Contracts.Abstractions;

public abstract class BaseSpecification<T>(Expression<Func<T, bool>> criteria)
{
    public Expression<Func<T, bool>> Criteria { get; } = criteria;
    public List<Expression<Func<T, object>>> Includes { get; } = [];

    protected void AddInclude(Expression<Func<T, object>> includeExpression) =>
        Includes.Add(includeExpression);
}
