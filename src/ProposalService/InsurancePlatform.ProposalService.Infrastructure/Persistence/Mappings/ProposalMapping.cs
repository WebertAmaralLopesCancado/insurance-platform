using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsurancePlatform.ProposalService.Infrastructure.Persistence.Mappings;

public sealed class ProposalMapping : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("proposals");

        builder.HasKey(proposal => proposal.Id);

        builder.Property(proposal => proposal.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.OwnsOne(proposal => proposal.CustomerName, customerName =>
        {
            customerName.Property(valueObject => valueObject.Value)
                .HasColumnName("customer_name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(proposal => proposal.InsuranceType, insuranceType =>
        {
            insuranceType.Property(valueObject => valueObject.Value)
                .HasColumnName("insurance_type")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(proposal => proposal.CoverageAmount, coverageAmount =>
        {
            coverageAmount.Property(valueObject => valueObject.Value)
                .HasColumnName("coverage_amount")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.Property(proposal => proposal.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(proposal => proposal.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Ignore(proposal => proposal.DomainEvents);
    }
}
