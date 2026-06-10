using FluentAssertions;
using InsurancePlatform.ProposalService.Domain.Exceptions;
using InsurancePlatform.ProposalService.Domain.ValueObjects;

namespace InsurancePlatform.ProposalService.Domain.UnitTests.ValueObjects;

public sealed class InsuranceTypeTests
{
    [Fact]
    public void Constructor_ShouldAcceptValidValue()
    {
        var insuranceType = new InsuranceType("Life");

        insuranceType.Value.Should().Be("Life");
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyValue()
    {
        var act = () => new InsuranceType(" ");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ShouldRejectNullValue()
    {
        var act = () => new InsuranceType(null!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ShouldRejectValueAboveOneHundredCharacters()
    {
        var value = new string('A', 101);

        var act = () => new InsuranceType(value);

        act.Should().Throw<DomainException>();
    }
}
