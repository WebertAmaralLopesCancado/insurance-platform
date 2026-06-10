using FluentAssertions;
using InsurancePlatform.ProposalService.Application.Exceptions;
using InsurancePlatform.ProposalService.Application.UseCases.RejectProposal;
using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.Repositories;
using InsurancePlatform.ProposalService.Domain.ValueObjects;
using Moq;

namespace InsurancePlatform.ProposalService.Application.UnitTests.UseCases.RejectProposal;

public sealed class RejectProposalCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldRejectExistingProposal()
    {
        var proposal = CreateProposal();
        var repository = new Mock<IProposalRepository>();
        repository
            .Setup(x => x.GetByIdAsync(proposal.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);
        var handler = new RejectProposalCommandHandler(repository.Object);

        var result = await handler.HandleAsync(new RejectProposalCommand(proposal.Id));

        result.IsSuccess.Should().BeTrue();
        proposal.Status.Should().Be(ProposalStatus.Rejected);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallProposalRepositoryUpdateAsync()
    {
        var proposal = CreateProposal();
        var repository = new Mock<IProposalRepository>();
        repository
            .Setup(x => x.GetByIdAsync(proposal.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);
        var handler = new RejectProposalCommandHandler(repository.Object);

        await handler.HandleAsync(new RejectProposalCommand(proposal.Id));

        repository.Verify(
            x => x.UpdateAsync(
                It.Is<Proposal>(updatedProposal =>
                    updatedProposal.Id == proposal.Id &&
                    updatedProposal.Status == ProposalStatus.Rejected),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundExceptionWhenProposalDoesNotExist()
    {
        var proposalId = Guid.NewGuid();
        var repository = new Mock<IProposalRepository>();
        repository
            .Setup(x => x.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Proposal?)null);
        var handler = new RejectProposalCommandHandler(repository.Object);

        var act = () => handler.HandleAsync(new RejectProposalCommand(proposalId));

        await act.Should().ThrowAsync<NotFoundException>();
        repository.Verify(
            x => x.UpdateAsync(It.IsAny<Proposal>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Proposal CreateProposal()
    {
        return Proposal.Create(
            new CustomerName("John Doe"),
            new InsuranceType("Auto"),
            new CoverageAmount(15000m));
    }
}
