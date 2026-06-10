using FluentAssertions;
using InsurancePlatform.ContractingService.Application.Exceptions;
using InsurancePlatform.ContractingService.Application.Ports;
using InsurancePlatform.ContractingService.Application.UseCases.CreateContract;
using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;
using InsurancePlatform.ContractingService.Domain.Repositories;
using Moq;

namespace InsurancePlatform.ContractingService.Application.UnitTests.UseCases.CreateContract;

public sealed class CreateContractCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateContractWhenProposalIsApproved()
    {
        var proposalId = Guid.NewGuid();
        var contractRepository = new Mock<IContractRepository>();
        var proposalServiceGateway = CreateProposalServiceGatewayMock(proposalId, "Approved");
        var handler = new CreateContractCommandHandler(
            contractRepository.Object,
            proposalServiceGateway.Object);

        var response = await handler.HandleAsync(new CreateContractCommand(proposalId));

        response.Id.Should().NotBeEmpty();
        response.ProposalId.Should().Be(proposalId);
        response.ContractedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallContractRepositoryAddAsync()
    {
        var proposalId = Guid.NewGuid();
        var contractRepository = new Mock<IContractRepository>();
        var proposalServiceGateway = CreateProposalServiceGatewayMock(proposalId, "Approved");
        var handler = new CreateContractCommandHandler(
            contractRepository.Object,
            proposalServiceGateway.Object);

        await handler.HandleAsync(new CreateContractCommand(proposalId));

        contractRepository.Verify(
            x => x.AddAsync(
                It.Is<Contract>(contract => contract.ProposalId == proposalId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallProposalServiceGatewayGetProposalByIdAsync()
    {
        var proposalId = Guid.NewGuid();
        var contractRepository = new Mock<IContractRepository>();
        var proposalServiceGateway = CreateProposalServiceGatewayMock(proposalId, "Approved");
        var handler = new CreateContractCommandHandler(
            contractRepository.Object,
            proposalServiceGateway.Object);

        await handler.HandleAsync(new CreateContractCommand(proposalId));

        proposalServiceGateway.Verify(
            x => x.GetProposalByIdAsync(proposalId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundExceptionWhenProposalDoesNotExist()
    {
        var proposalId = Guid.NewGuid();
        var contractRepository = new Mock<IContractRepository>();
        var proposalServiceGateway = new Mock<IProposalServiceGateway>();
        proposalServiceGateway
            .Setup(x => x.GetProposalByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProposalSnapshot?)null);
        var handler = new CreateContractCommandHandler(
            contractRepository.Object,
            proposalServiceGateway.Object);

        var act = () => handler.HandleAsync(new CreateContractCommand(proposalId));

        await act.Should().ThrowAsync<NotFoundException>();
        contractRepository.Verify(
            x => x.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowProposalNotApprovedExceptionWhenProposalIsNotApproved()
    {
        var proposalId = Guid.NewGuid();
        var contractRepository = new Mock<IContractRepository>();
        var proposalServiceGateway = CreateProposalServiceGatewayMock(proposalId, "UnderAnalysis");
        var handler = new CreateContractCommandHandler(
            contractRepository.Object,
            proposalServiceGateway.Object);

        var act = () => handler.HandleAsync(new CreateContractCommand(proposalId));

        await act.Should().ThrowAsync<ProposalNotApprovedException>();
        contractRepository.Verify(
            x => x.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowProposalAlreadyContractedExceptionWhenContractAlreadyExistsForProposalId()
    {
        var proposalId = Guid.NewGuid();
        var contractRepository = new Mock<IContractRepository>();
        contractRepository
            .Setup(x => x.ExistsByProposalIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var proposalServiceGateway = CreateProposalServiceGatewayMock(proposalId, "Approved");
        var handler = new CreateContractCommandHandler(
            contractRepository.Object,
            proposalServiceGateway.Object);

        var act = () => handler.HandleAsync(new CreateContractCommand(proposalId));

        await act.Should().ThrowAsync<ProposalAlreadyContractedException>();
        contractRepository.Verify(
            x => x.AddAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<IProposalServiceGateway> CreateProposalServiceGatewayMock(
        Guid proposalId,
        string status)
    {
        var proposalServiceGateway = new Mock<IProposalServiceGateway>();
        proposalServiceGateway
            .Setup(x => x.GetProposalByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProposalSnapshot(proposalId, status));

        return proposalServiceGateway;
    }
}
