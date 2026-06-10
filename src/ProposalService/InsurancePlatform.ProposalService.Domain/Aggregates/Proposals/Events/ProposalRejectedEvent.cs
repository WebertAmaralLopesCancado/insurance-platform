using InsurancePlatform.ProposalService.Domain.SeedWork;

namespace InsurancePlatform.ProposalService.Domain.Aggregates.Proposals.Events;

public sealed record ProposalRejectedEvent(
    Guid ProposalId,
    DateTime OccurredOnUtc) : IDomainEvent;
