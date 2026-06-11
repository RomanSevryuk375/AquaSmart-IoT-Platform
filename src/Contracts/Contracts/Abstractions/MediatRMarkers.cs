using Contracts.Results;
using MediatR;

namespace Contracts.Abstractions;

public interface IBaseCommand { }

public interface ICommand : IRequest<Result>, IBaseCommand { }

public interface ICommand<TValue> : IRequest<Result<TValue>>, IBaseCommand { }

public interface IQuery<TResponse> : IRequest<TResponse> { }
