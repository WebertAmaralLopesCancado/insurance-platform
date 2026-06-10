using InsurancePlatform.ProposalService.Application.Common;

namespace InsurancePlatform.ProposalService.Application.UseCases.CreateProposal;

public sealed record CreateProposalCommand(
    string CustomerName,
    string InsuranceType,
    decimal CoverageAmount) : ICommand;
