using InsurancePlatform.ProposalService.Application.Common;

namespace InsurancePlatform.ProposalService.Application.UseCases.ApproveProposal;

public sealed record ApproveProposalCommand(Guid Id) : ICommand;
