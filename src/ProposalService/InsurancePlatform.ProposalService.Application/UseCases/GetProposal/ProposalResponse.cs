namespace InsurancePlatform.ProposalService.Application.UseCases.GetProposal;

public sealed record ProposalResponse(
    Guid Id,
    string CustomerName,
    string InsuranceType,
    decimal CoverageAmount,
    string Status,
    DateTime CreatedAt);
