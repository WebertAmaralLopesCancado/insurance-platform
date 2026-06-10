using InsurancePlatform.ProposalService.Domain.SeedWork;

namespace InsurancePlatform.ProposalService.Domain.Aggregates.Proposals.Events;

public sealed record ProposalApprovedEvent(
    Guid ProposalId,
    DateTime OccurredOnUtc) : IDomainEvent;
