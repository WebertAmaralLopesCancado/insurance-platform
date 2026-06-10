namespace InsurancePlatform.ProposalService.Application.UseCases.CreateProposal;

public sealed record CreateProposalResponse(
    Guid Id,
    string CustomerName,
    string InsuranceType,
    decimal CoverageAmount,
    string Status,
    DateTime CreatedAt);
