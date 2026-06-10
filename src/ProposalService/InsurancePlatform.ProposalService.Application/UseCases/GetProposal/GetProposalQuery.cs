using InsurancePlatform.ProposalService.Application.Common;

namespace InsurancePlatform.ProposalService.Application.UseCases.GetProposal;

public sealed record GetProposalQuery(Guid Id) : IQuery;
