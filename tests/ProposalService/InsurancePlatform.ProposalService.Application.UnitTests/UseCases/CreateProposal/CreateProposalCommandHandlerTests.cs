using FluentAssertions;
using InsurancePlatform.ProposalService.Application.UseCases.CreateProposal;
using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.Repositories;
using Moq;

namespace InsurancePlatform.ProposalService.Application.UnitTests.UseCases.CreateProposal;

public sealed class CreateProposalCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateProposalAndReturnId()
    {
        var repository = new Mock<IProposalRepository>();
        var handler = new CreateProposalCommandHandler(repository.Object);
        var command = new CreateProposalCommand("John Doe", "Auto", 15000m);

        var response = await handler.HandleAsync(command);

        response.Id.Should().NotBeEmpty();
        response.CustomerName.Should().Be(command.CustomerName);
        response.InsuranceType.Should().Be(command.InsuranceType);
        response.CoverageAmount.Should().Be(command.CoverageAmount);
        response.Status.Should().Be(ProposalStatus.UnderAnalysis.ToString());
        response.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallProposalRepositoryAddAsync()
    {
        var repository = new Mock<IProposalRepository>();
        var handler = new CreateProposalCommandHandler(repository.Object);
        var command = new CreateProposalCommand("John Doe", "Auto", 15000m);

        await handler.HandleAsync(command);

        repository.Verify(
            x => x.AddAsync(
                It.Is<Proposal>(proposal =>
                    proposal.CustomerName.Value == command.CustomerName &&
                    proposal.InsuranceType.Value == command.InsuranceType &&
                    proposal.CoverageAmount.Value == command.CoverageAmount),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
