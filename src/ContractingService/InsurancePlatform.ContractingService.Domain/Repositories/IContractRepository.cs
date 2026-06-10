using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;

namespace InsurancePlatform.ContractingService.Domain.Repositories;

public interface IContractRepository
{
    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);

    Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByProposalIdAsync(Guid proposalId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Contract contract, CancellationToken cancellationToken = default);
}
