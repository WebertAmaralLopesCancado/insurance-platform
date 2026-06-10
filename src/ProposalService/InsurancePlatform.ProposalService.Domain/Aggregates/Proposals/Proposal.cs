using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals.Events;
using InsurancePlatform.ProposalService.Domain.Exceptions;
using InsurancePlatform.ProposalService.Domain.SeedWork;
using InsurancePlatform.ProposalService.Domain.ValueObjects;

namespace InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;

public sealed class Proposal : AggregateRoot<Guid>
{
    private Proposal()
    {
    }

    private Proposal(
        Guid id,
        CustomerName customerName,
        InsuranceType insuranceType,
        CoverageAmount coverageAmount,
        DateTime createdAt)
    {
        Id = id;
        CustomerName = customerName;
        InsuranceType = insuranceType;
        CoverageAmount = coverageAmount;
        CreatedAt = createdAt;
        Status = ProposalStatus.UnderAnalysis;
    }

    public CustomerName CustomerName { get; private set; } = default!;

    public InsuranceType InsuranceType { get; private set; } = default!;

    public CoverageAmount CoverageAmount { get; private set; } = default!;

    public ProposalStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Proposal Create(
        CustomerName customerName,
        InsuranceType insuranceType,
        CoverageAmount coverageAmount)
    {
        ArgumentNullException.ThrowIfNull(customerName);
        ArgumentNullException.ThrowIfNull(insuranceType);
        ArgumentNullException.ThrowIfNull(coverageAmount);

        var proposal = new Proposal(
            Guid.NewGuid(),
            customerName,
            insuranceType,
            coverageAmount,
            DateTime.UtcNow);

        proposal.AddDomainEvent(new ProposalCreatedEvent(
            proposal.Id,
            proposal.CustomerName.Value,
            proposal.InsuranceType.Value,
            proposal.CoverageAmount.Value,
            DateTime.UtcNow));

        return proposal;
    }

    public void Approve()
    {
        EnsureCanChangeStatus("approve");

        Status = ProposalStatus.Approved;

        AddDomainEvent(new ProposalApprovedEvent(Id, DateTime.UtcNow));
    }

    public void Reject()
    {
        EnsureCanChangeStatus("reject");

        Status = ProposalStatus.Rejected;

        AddDomainEvent(new ProposalRejectedEvent(Id, DateTime.UtcNow));
    }

    private void EnsureCanChangeStatus(string action)
    {
        if (Status != ProposalStatus.UnderAnalysis)
        {
            throw new DomainException($"Cannot {action} a proposal with status '{Status}'.");
        }
    }
}
