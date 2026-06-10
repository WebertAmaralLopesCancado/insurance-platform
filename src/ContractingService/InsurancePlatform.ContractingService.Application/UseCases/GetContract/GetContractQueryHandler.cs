using InsurancePlatform.ContractingService.Application.Common;
using InsurancePlatform.ContractingService.Application.Exceptions;
using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;
using InsurancePlatform.ContractingService.Domain.Repositories;

namespace InsurancePlatform.ContractingService.Application.UseCases.GetContract;

public sealed class GetContractQueryHandler
    : IQueryHandler<GetContractQuery, ContractResponse>
{
    private readonly IContractRepository _contractRepository;

    public GetContractQueryHandler(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<ContractResponse> HandleAsync(
        GetContractQuery query,
        CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdAsync(query.Id, cancellationToken);

        if (contract is null)
        {
            throw new NotFoundException($"Contract with id '{query.Id}' was not found.");
        }

        return MapToResponse(contract);
    }

    private static ContractResponse MapToResponse(Contract contract)
    {
        return new ContractResponse(
            contract.Id,
            contract.ProposalId,
            contract.ContractedAt);
    }
}
