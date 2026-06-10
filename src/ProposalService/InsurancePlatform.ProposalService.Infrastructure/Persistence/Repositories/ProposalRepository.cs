using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.Repositories;
using InsurancePlatform.ProposalService.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace InsurancePlatform.ProposalService.Infrastructure.Persistence.Repositories;

public sealed class ProposalRepository : IProposalRepository
{
    private readonly ProposalDbContext _dbContext;

    public ProposalRepository(ProposalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default)
    {
        await _dbContext.Proposals.AddAsync(proposal, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Proposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Proposals
            .AsNoTracking()
            .FirstOrDefaultAsync(proposal => proposal.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Proposals
            .AsNoTracking()
            .AnyAsync(proposal => proposal.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Proposal proposal, CancellationToken cancellationToken = default)
    {
        _dbContext.Proposals.Update(proposal);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<Proposal>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Proposals
            .AsNoTracking()
            .OrderByDescending(proposal => proposal.CreatedAt)
            .ThenBy(proposal => proposal.Id);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<Proposal>(items, pageNumber, pageSize, totalItems);
    }
}
