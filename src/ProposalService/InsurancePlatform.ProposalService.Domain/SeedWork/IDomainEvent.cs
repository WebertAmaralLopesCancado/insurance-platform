namespace InsurancePlatform.ProposalService.Domain.SeedWork;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
