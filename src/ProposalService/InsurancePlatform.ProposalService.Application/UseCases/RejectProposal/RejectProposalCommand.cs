using InsurancePlatform.ProposalService.Application.Common;

namespace InsurancePlatform.ProposalService.Application.UseCases.RejectProposal;

public sealed record RejectProposalCommand(Guid Id) : ICommand;
