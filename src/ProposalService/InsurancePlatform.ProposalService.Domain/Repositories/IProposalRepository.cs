using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.SeedWork;

namespace InsurancePlatform.ProposalService.Domain.Repositories;

public interface IProposalRepository
{
    Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default);

    Task<Proposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateAsync(Proposal proposal, CancellationToken cancellationToken = default);

    Task<PagedResult<Proposal>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
