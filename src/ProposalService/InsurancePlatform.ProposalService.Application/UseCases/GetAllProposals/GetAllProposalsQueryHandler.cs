using InsurancePlatform.ProposalService.Application.Common;
using InsurancePlatform.ProposalService.Domain.Aggregates.Proposals;
using InsurancePlatform.ProposalService.Domain.Repositories;

namespace InsurancePlatform.ProposalService.Application.UseCases.GetAllProposals;

public sealed class GetAllProposalsQueryHandler
    : IQueryHandler<GetAllProposalsQuery, PagedResponse<ProposalListResponse>>
{
    private readonly IProposalRepository _proposalRepository;

    public GetAllProposalsQueryHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<PagedResponse<ProposalListResponse>> HandleAsync(
        GetAllProposalsQuery query,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _proposalRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var items = pagedResult.Items
            .Select(MapToResponse)
            .ToArray();

        return new PagedResponse<ProposalListResponse>(
            items,
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalItems);
    }

    private static ProposalListResponse MapToResponse(Proposal proposal)
    {
        return new ProposalListResponse(
            proposal.Id,
            proposal.CustomerName.Value,
            proposal.InsuranceType.Value,
            proposal.CoverageAmount.Value,
            proposal.Status.ToString(),
            proposal.CreatedAt);
    }
}
