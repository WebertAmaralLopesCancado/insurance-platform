using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts.Events;
using InsurancePlatform.ContractingService.Domain.Exceptions;
using InsurancePlatform.ContractingService.Domain.SeedWork;

namespace InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;

public sealed class Contract : AggregateRoot<Guid>
{
    private Contract()
    {
    }

    private Contract(Guid id, Guid proposalId, DateTime contractedAt)
    {
        Id = id;
        ProposalId = proposalId;
        ContractedAt = contractedAt;
    }

    public Guid ProposalId { get; private set; }

    public DateTime ContractedAt { get; private set; }

    public static Contract Create(Guid proposalId)
    {
        if (proposalId == Guid.Empty)
        {
            throw new DomainException("Proposal id is required.");
        }

        var contractedAt = DateTime.UtcNow;
        var contract = new Contract(Guid.NewGuid(), proposalId, contractedAt);

        contract.AddDomainEvent(new ContractCreatedEvent(
            contract.Id,
            contract.ProposalId,
            contract.ContractedAt,
            DateTime.UtcNow));

        return contract;
    }
}
