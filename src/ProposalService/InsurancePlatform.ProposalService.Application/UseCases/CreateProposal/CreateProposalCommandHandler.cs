using InsurancePlatform.ProposalService.Application.Common;
using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.Repositories;
using InsurancePlatform.ProposalService.Domain.ValueObjects;

namespace InsurancePlatform.ProposalService.Application.UseCases.CreateProposal;

public sealed class CreateProposalCommandHandler
    : ICommandHandler<CreateProposalCommand, CreateProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;

    public CreateProposalCommandHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<CreateProposalResponse> HandleAsync(
        CreateProposalCommand command,
        CancellationToken cancellationToken = default)
    {
        var proposal = Proposal.Create(
            new CustomerName(command.CustomerName),
            new InsuranceType(command.InsuranceType),
            new CoverageAmount(command.CoverageAmount));

        await _proposalRepository.AddAsync(proposal, cancellationToken);

        return new CreateProposalResponse(
            proposal.Id,
            proposal.CustomerName.Value,
            proposal.InsuranceType.Value,
            proposal.CoverageAmount.Value,
            proposal.Status.ToString(),
            proposal.CreatedAt);
    }
}
