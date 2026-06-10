using FluentAssertions;
using InsurancePlatform.ProposalService.Domain.Exceptions;
using InsurancePlatform.ProposalService.Domain.ValueObjects;

namespace InsurancePlatform.ProposalService.Domain.UnitTests.ValueObjects;

public sealed class CustomerNameTests
{
    [Fact]
    public void Constructor_ShouldAcceptValidValue()
    {
        var customerName = new CustomerName("John Doe");

        customerName.Value.Should().Be("John Doe");
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyValue()
    {
        var act = () => new CustomerName(" ");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ShouldRejectNullValue()
    {
        var act = () => new CustomerName(null!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ShouldRejectValueAboveTwoHundredCharacters()
    {
        var value = new string('A', 201);

        var act = () => new CustomerName(value);

        act.Should().Throw<DomainException>();
    }
}
