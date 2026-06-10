namespace InsurancePlatform.ContractingService.Application.Common;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
