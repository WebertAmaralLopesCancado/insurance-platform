using InsurancePlatform.ProposalService.Application.Common;

namespace InsurancePlatform.ProposalService.Application.UseCases.GetAllProposals;

public sealed record GetAllProposalsQuery(
    int PageNumber = 1,
    int PageSize = 10) : IQuery;
