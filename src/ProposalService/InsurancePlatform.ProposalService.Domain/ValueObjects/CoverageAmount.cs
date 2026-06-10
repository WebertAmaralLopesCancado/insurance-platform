using InsurancePlatform.ProposalService.Domain.Exceptions;
using InsurancePlatform.ProposalService.Domain.SeedWork;

namespace InsurancePlatform.ProposalService.Domain.ValueObjects;

public sealed class CoverageAmount : ValueObject
{
    public CoverageAmount(decimal value)
    {
        if (value <= 0)
        {
            throw new DomainException("Coverage amount must be greater than zero.");
        }

        Value = value;
    }

    public decimal Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
