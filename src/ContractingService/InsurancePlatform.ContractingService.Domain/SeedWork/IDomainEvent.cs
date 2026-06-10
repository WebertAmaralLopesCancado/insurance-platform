namespace InsurancePlatform.ContractingService.Domain.SeedWork;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
