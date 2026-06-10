using InsurancePlatform.ProposalService.Domain.Exceptions;
using InsurancePlatform.ProposalService.Domain.SeedWork;

namespace InsurancePlatform.ProposalService.Domain.ValueObjects;

public sealed class InsuranceType : ValueObject
{
    private const int MaxLength = 100;

    public InsuranceType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Insurance type is required.");
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            throw new DomainException($"Insurance type must not exceed {MaxLength} characters.");
        }

        Value = normalizedValue;
    }

    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
