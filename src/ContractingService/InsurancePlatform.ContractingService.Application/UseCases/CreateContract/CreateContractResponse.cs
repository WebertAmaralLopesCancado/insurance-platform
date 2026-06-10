namespace InsurancePlatform.ContractingService.Application.UseCases.CreateContract;

public sealed record CreateContractResponse(
    Guid Id,
    Guid ProposalId,
    DateTime ContractedAt);
