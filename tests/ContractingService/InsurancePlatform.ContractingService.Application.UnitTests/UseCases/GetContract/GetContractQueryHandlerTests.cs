using FluentAssertions;
using InsurancePlatform.ContractingService.Application.Exceptions;
using InsurancePlatform.ContractingService.Application.UseCases.GetContract;
using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;
using InsurancePlatform.ContractingService.Domain.Repositories;
using Moq;

namespace InsurancePlatform.ContractingService.Application.UnitTests.UseCases.GetContract;

public sealed class GetContractQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnContractWhenFound()
    {
        var contract = Contract.Create(Guid.NewGuid());
        var contractRepository = new Mock<IContractRepository>();
        contractRepository
            .Setup(x => x.GetByIdAsync(contract.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);
        var handler = new GetContractQueryHandler(contractRepository.Object);

        var response = await handler.HandleAsync(new GetContractQuery(contract.Id));

        response.Id.Should().Be(contract.Id);
        response.ProposalId.Should().Be(contract.ProposalId);
        response.ContractedAt.Should().Be(contract.ContractedAt);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFoundExceptionWhenContractIsNotFound()
    {
        var contractId = Guid.NewGuid();
        var contractRepository = new Mock<IContractRepository>();
        contractRepository
            .Setup(x => x.GetByIdAsync(contractId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);
        var handler = new GetContractQueryHandler(contractRepository.Object);

        var act = () => handler.HandleAsync(new GetContractQuery(contractId));

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
