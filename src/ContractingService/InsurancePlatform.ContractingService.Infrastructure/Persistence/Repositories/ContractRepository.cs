using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;
using InsurancePlatform.ContractingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InsurancePlatform.ContractingService.Infrastructure.Persistence.Repositories;

public sealed class ContractRepository : IContractRepository
{
    private readonly ContractingDbContext _dbContext;

    public ContractRepository(ContractingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        await _dbContext.Contracts.AddAsync(contract, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Contracts
            .AsNoTracking()
            .FirstOrDefaultAsync(contract => contract.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByProposalIdAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Contracts
            .AsNoTracking()
            .AnyAsync(contract => contract.ProposalId == proposalId, cancellationToken);
    }

    public async Task UpdateAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        _dbContext.Contracts.Update(contract);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
