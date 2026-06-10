using FluentAssertions;
using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals.Events;
using InsurancePlatform.ProposalService.Domain.Exceptions;
using InsurancePlatform.ProposalService.Domain.ValueObjects;

namespace InsurancePlatform.ProposalService.Domain.UnitTests.Aggregates.Proposals;

public sealed class ProposalTests
{
    [Fact]
    public void Create_ShouldStartUnderAnalysis()
    {
        var proposal = CreateProposal();

        proposal.Status.Should().Be(ProposalStatus.UnderAnalysis);
    }

    [Fact]
    public void Create_ShouldFillId()
    {
        var proposal = CreateProposal();

        proposal.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ShouldFillCreatedAt()
    {
        var beforeCreation = DateTime.UtcNow;

        var proposal = CreateProposal();

        proposal.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        proposal.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldRegisterProposalCreatedEvent()
    {
        var proposal = CreateProposal();

        var domainEvent = proposal.DomainEvents.Should().ContainSingle().Subject;
        domainEvent.Should().BeOfType<ProposalCreatedEvent>();

        var proposalCreatedEvent = (ProposalCreatedEvent)domainEvent;
        proposalCreatedEvent.OccurredOnUtc.Should().Be(proposal.CreatedAt);
    }

    [Fact]
    public void Approve_ShouldChangeStatusToApproved()
    {
        var proposal = CreateProposal();

        proposal.Approve();

        proposal.Status.Should().Be(ProposalStatus.Approved);
    }

    [Fact]
    public void Approve_ShouldRegisterProposalApprovedEvent()
    {
        var proposal = CreateProposal();
        proposal.ClearDomainEvents();

        proposal.Approve();

        proposal.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProposalApprovedEvent>();
    }

    [Fact]
    public void Approve_ShouldNotAllowApprovingTwice()
    {
        var proposal = CreateProposal();
        proposal.Approve();

        var act = () => proposal.Approve();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_ShouldChangeStatusToRejected()
    {
        var proposal = CreateProposal();

        proposal.Reject();

        proposal.Status.Should().Be(ProposalStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldRegisterProposalRejectedEvent()
    {
        var proposal = CreateProposal();
        proposal.ClearDomainEvents();

        proposal.Reject();

        proposal.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProposalRejectedEvent>();
    }

    [Fact]
    public void Reject_ShouldNotAllowRejectingTwice()
    {
        var proposal = CreateProposal();
        proposal.Reject();

        var act = () => proposal.Reject();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_ShouldNotAllowRejectingAfterApproval()
    {
        var proposal = CreateProposal();
        proposal.Approve();

        var act = () => proposal.Reject();

        act.Should().Throw<DomainException>();
    }

    private static Proposal CreateProposal()
    {
        return Proposal.Create(
            new CustomerName("John Doe"),
            new InsuranceType("Life"),
            new CoverageAmount(1000m));
    }
}
