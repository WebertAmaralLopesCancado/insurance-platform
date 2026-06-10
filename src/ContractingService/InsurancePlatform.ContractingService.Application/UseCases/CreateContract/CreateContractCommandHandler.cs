using InsurancePlatform.ContractingService.Application.Common;
using InsurancePlatform.ContractingService.Application.Exceptions;
using InsurancePlatform.ContractingService.Application.Ports;
using InsurancePlatform.ContractingService.Domain.Aggregates.Contracts;
using InsurancePlatform.ContractingService.Domain.Repositories;

namespace InsurancePlatform.ContractingService.Application.UseCases.CreateContract;

public sealed class CreateContractCommandHandler
    : ICommandHandler<CreateContractCommand, CreateContractResponse>
{
    private const string ApprovedStatus = "Approved";

    private readonly IContractRepository _contractRepository;
    private readonly IProposalServiceGateway _proposalServiceGateway;

    public CreateContractCommandHandler(
        IContractRepository contractRepository,
        IProposalServiceGateway proposalServiceGateway)
    {
        _contractRepository = contractRepository;
        _proposalServiceGateway = proposalServiceGateway;
    }

    public async Task<CreateContractResponse> HandleAsync(
        CreateContractCommand command,
        CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalServiceGateway.GetProposalByIdAsync(
            command.ProposalId,
            cancellationToken);

        if (proposal is null)
        {
            throw new NotFoundException($"Proposal with id '{command.ProposalId}' was not found.");
        }

        if (!string.Equals(proposal.Status, ApprovedStatus, StringComparison.Ordinal))
        {
            throw new ProposalNotApprovedException(
                $"Proposal with id '{command.ProposalId}' is not approved.");
        }

        var alreadyContracted = await _contractRepository.ExistsByProposalIdAsync(
            command.ProposalId,
            cancellationToken);

        if (alreadyContracted)
        {
            throw new ProposalAlreadyContractedException(
                $"Proposal with id '{command.ProposalId}' is already contracted.");
        }

        var contract = Contract.Create(command.ProposalId);

        await _contractRepository.AddAsync(contract, cancellationToken);

        return new CreateContractResponse(
            contract.Id,
            contract.ProposalId,
            contract.ContractedAt);
    }
}
