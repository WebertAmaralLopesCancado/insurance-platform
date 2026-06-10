using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using Microsoft.EntityFrameworkCore;

namespace InsurancePlatform.ProposalService.Infrastructure.Persistence;

public sealed class ProposalDbContext : DbContext
{
    public ProposalDbContext(DbContextOptions<ProposalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Proposal> Proposals => Set<Proposal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProposalDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
