using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;
using Microsoft.EntityFrameworkCore;

namespace InsurancePlatform.ContractingService.Infrastructure.Persistence;

public sealed class ContractingDbContext : DbContext
{
    public ContractingDbContext(DbContextOptions<ContractingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Contract> Contracts => Set<Contract>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContractingDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
