using InsurancePlatform.ContractingService.Domain.SeedWork;

namespace InsurancePlatform.ContractingService.Domain.Aggregates.Contracts.Events;

public sealed record ContractCreatedEvent(
    Guid ContractId,
    Guid ProposalId,
    DateTime ContractedAt,
    DateTime OccurredOnUtc) : IDomainEvent;
