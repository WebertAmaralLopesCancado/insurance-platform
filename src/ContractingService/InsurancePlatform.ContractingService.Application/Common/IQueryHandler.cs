namespace InsurancePlatform.ContractingService.Application.Common;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
