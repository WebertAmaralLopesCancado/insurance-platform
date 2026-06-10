using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsurancePlatform.ContractingService.Infrastructure.Persistence.Mappings;

public sealed class ContractMapping : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");

        builder.HasKey(contract => contract.Id);

        builder.Property(contract => contract.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(contract => contract.ProposalId)
            .HasColumnName("proposal_id")
            .IsRequired();

        builder.Property(contract => contract.ContractedAt)
            .HasColumnName("contracted_at")
            .IsRequired();

        builder.HasIndex(contract => contract.ProposalId)
            .IsUnique();

        builder.Ignore(contract => contract.DomainEvents);
    }
}
