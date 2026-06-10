using InsurancePlatform.ProposalService.Domain.SeedWork;

namespace InsurancePlatform.ProposalService.Domain.Aggregates.Proposals.Events;

public sealed record ProposalCreatedEvent(
    Guid ProposalId,
    string CustomerName,
    string InsuranceType,
    decimal CoverageAmount,
    DateTime OccurredOnUtc) : IDomainEvent;
