using FluentAssertions;
using InsurancePlatform.ProposalService.Domain.Exceptions;
using InsurancePlatform.ProposalService.Domain.ValueObjects;

namespace InsurancePlatform.ProposalService.Domain.UnitTests.ValueObjects;

public sealed class CoverageAmountTests
{
    [Fact]
    public void Constructor_ShouldAcceptPositiveValue()
    {
        var coverageAmount = new CoverageAmount(1000m);

        coverageAmount.Value.Should().Be(1000m);
    }

    [Fact]
    public void Constructor_ShouldRejectZero()
    {
        var act = () => new CoverageAmount(0m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeValue()
    {
        var act = () => new CoverageAmount(-1m);

        act.Should().Throw<DomainException>();
    }
}
