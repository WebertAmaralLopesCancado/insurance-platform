using FluentAssertions;
using InsurancePlatform.ProposalService.Application.Exceptions;
using InsurancePlatform.ProposalService.Application.UseCases.GetProposal;
using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.Repositories;
using InsurancePlatform.ProposalService.Domain.ValueObjects;
using Moq;

namespace InsurancePlatform.ProposalService.Application.UnitTests.UseCases.GetProposal;

public sealed class GetProposalQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnProposalWhenFound()
    {
        var proposal = CreateProposal();
        var repository = new Mock<IProposalRepository>();
        repository
            .Setup(x => x.GetByIdAsync(proposal.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);
        var handler = new GetProposalQueryHandler(repository.Object);

        var response = await handler.HandleAsync(new GetProposalQuery(proposal.Id));

        response.Id.Should().Be(proposal.Id);
        response.CustomerName.Should().Be(proposal.CustomerName.Value);
        response.InsuranceType.Should().Be(proposal.InsuranceType.Value);
        response.CoverageAmount.Should().Be(proposal.CoverageAmount.Value);
        response.Status.Should().Be(proposal.Status.ToString());
        response.CreatedAt.Should().Be(proposal.CreatedAt);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundExceptionWhenProposalIsNotFound()
    {
        var proposalId = Guid.NewGuid();
        var repository = new Mock<IProposalRepository>();
        repository
            .Setup(x => x.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Proposal?)null);
        var handler = new GetProposalQueryHandler(repository.Object);

        var act = () => handler.HandleAsync(new GetProposalQuery(proposalId));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static Proposal CreateProposal()
    {
        return Proposal.Create(
            new CustomerName("John Doe"),
            new InsuranceType("Auto"),
            new CoverageAmount(15000m));
    }
}
