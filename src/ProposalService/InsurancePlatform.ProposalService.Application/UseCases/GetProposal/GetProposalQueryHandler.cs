using InsurancePlatform.ProposalService.Application.Common;
using InsurancePlatform.ProposalService.Application.Exceptions;
using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.Repositories;

namespace InsurancePlatform.ProposalService.Application.UseCases.GetProposal;

public sealed class GetProposalQueryHandler
    : IQueryHandler<GetProposalQuery, ProposalResponse>
{
    private readonly IProposalRepository _proposalRepository;

    public GetProposalQueryHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<ProposalResponse> HandleAsync(
        GetProposalQuery query,
        CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(query.Id, cancellationToken);

        if (proposal is null)
        {
            throw new NotFoundException($"Proposal with id '{query.Id}' was not found.");
        }

        return MapToResponse(proposal);
    }

    private static ProposalResponse MapToResponse(Proposal proposal)
    {
        return new ProposalResponse(
            proposal.Id,
            proposal.CustomerName.Value,
            proposal.InsuranceType.Value,
            proposal.CoverageAmount.Value,
            proposal.Status.ToString(),
            proposal.CreatedAt);
    }
}
