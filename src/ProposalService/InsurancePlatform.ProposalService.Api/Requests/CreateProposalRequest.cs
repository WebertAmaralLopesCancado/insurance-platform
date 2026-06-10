namespace InsurancePlatform.ProposalService.Api.Requests;

public sealed record CreateProposalRequest(
    string CustomerName,
    string InsuranceType,
    decimal CoverageAmount);
