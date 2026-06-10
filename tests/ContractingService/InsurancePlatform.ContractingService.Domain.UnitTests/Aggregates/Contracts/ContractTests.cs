using FluentAssertions;
using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;
using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts.Events;
using InsurancePlatform.ContractingService.Domain.Exceptions;

namespace InsurancePlatform.ContractingService.Domain.UnitTests.Aggregates.Contracts;

public sealed class ContractTests
{
    [Fact]
    public void Create_ShouldCreateContractWithValidProposalId()
    {
        var proposalId = Guid.NewGuid();

        var contract = Contract.Create(proposalId);

        contract.ProposalId.Should().Be(proposalId);
    }

    [Fact]
    public void Create_ShouldFillId()
    {
        var contract = Contract.Create(Guid.NewGuid());

        contract.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_ShouldFillContractedAt()
    {
        var beforeCreation = DateTime.UtcNow;

        var contract = Contract.Create(Guid.NewGuid());

        contract.ContractedAt.Should().BeOnOrAfter(beforeCreation);
        contract.ContractedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldRegisterContractCreatedEvent()
    {
        var proposalId = Guid.NewGuid();

        var contract = Contract.Create(proposalId);

        var domainEvent = contract.DomainEvents.Should().ContainSingle().Subject;
        domainEvent.Should().BeOfType<ContractCreatedEvent>();

        var contractCreatedEvent = (ContractCreatedEvent)domainEvent;
        contractCreatedEvent.ContractId.Should().Be(contract.Id);
        contractCreatedEvent.ProposalId.Should().Be(proposalId);
        contractCreatedEvent.ContractedAt.Should().Be(contract.ContractedAt);
        contractCreatedEvent.OccurredOnUtc.Should().Be(contract.ContractedAt);
    }

    [Fact]
    public void Create_ShouldRejectEmptyProposalId()
    {
        var act = () => Contract.Create(Guid.Empty);

        act.Should().Throw<DomainException>();
    }
}
