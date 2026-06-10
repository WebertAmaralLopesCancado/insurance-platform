namespace InsurancePlatform.ProposalService.Application.UseCases.GetAllProposals;

public sealed record ProposalListResponse(
    Guid Id,
    string CustomerName,
    string InsuranceType,
    decimal CoverageAmount,
    string Status,
    DateTime CreatedAt);
