using InsurancePlatform.ProposalService.Domain.Exceptions;
using InsurancePlatform.ProposalService.Domain.SeedWork;

namespace InsurancePlatform.ProposalService.Domain.ValueObjects;

public sealed class CustomerName : ValueObject
{
    private const int MaxLength = 200;

    public CustomerName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Customer name is required.");
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > MaxLength)
        {
            throw new DomainException($"Customer name must not exceed {MaxLength} characters.");
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
