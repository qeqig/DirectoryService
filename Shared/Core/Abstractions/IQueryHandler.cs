using CSharpFunctionalExtensions;

namespace Core.Abstractions;

public interface IQueryHandler<TResponse, in TQuery>
    where TQuery : IQuery
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken = default);
}