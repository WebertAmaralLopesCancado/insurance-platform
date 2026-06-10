namespace InsurancePlatform.ContractingService.Application.UseCases.GetContract;

public sealed record ContractResponse(
    Guid Id,
    Guid ProposalId,
    DateTime ContractedAt);
