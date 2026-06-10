using FluentAssertions;
using InsurancePlatform.ProposalService.Application.UseCases.GetAllProposals;
using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.Repositories;
using InsurancePlatform.ProposalService.Domain.SeedWork;
using InsurancePlatform.ProposalService.Domain.ValueObjects;
using Moq;

namespace InsurancePlatform.ProposalService.Application.UnitTests.UseCases.GetAllProposals;

public sealed class GetAllProposalsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnPagedProposals()
    {
        var proposals = new[]
        {
            CreateProposal("John Doe", "Auto", 15000m),
            CreateProposal("Jane Doe", "Home", 250000m)
        };
        var pagedResult = new PagedResult<Proposal>(proposals, pageNumber: 2, pageSize: 2, totalItems: 5);
        var repository = new Mock<IProposalRepository>();
        repository
            .Setup(x => x.GetPagedAsync(2, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);
        var handler = new GetAllProposalsQueryHandler(repository.Object);

        var response = await handler.HandleAsync(new GetAllProposalsQuery(2, 2));

        response.Items.Should().HaveCount(2);
        response.Items.Select(x => x.Id).Should().BeEquivalentTo(proposals.Select(x => x.Id));
    }

    [Fact]
    public async Task HandleAsync_ShouldPreservePaginationMetadata()
    {
        var proposals = new[]
        {
            CreateProposal("John Doe", "Auto", 15000m),
            CreateProposal("Jane Doe", "Home", 250000m)
        };
        var pagedResult = new PagedResult<Proposal>(proposals, pageNumber: 2, pageSize: 2, totalItems: 5);
        var repository = new Mock<IProposalRepository>();
        repository
            .Setup(x => x.GetPagedAsync(2, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);
        var handler = new GetAllProposalsQueryHandler(repository.Object);

        var response = await handler.HandleAsync(new GetAllProposalsQuery(2, 2));

        response.PageNumber.Should().Be(2);
        response.PageSize.Should().Be(2);
        response.TotalItems.Should().Be(5);
        response.TotalPages.Should().Be(3);
    }

    private static Proposal CreateProposal(string customerName, string insuranceType, decimal coverageAmount)
    {
        return Proposal.Create(
            new CustomerName(customerName),
            new InsuranceType(insuranceType),
            new CoverageAmount(coverageAmount));
    }
}
