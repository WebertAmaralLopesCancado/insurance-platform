using InsurancePlatform.ContractingService.Application.Common;

namespace InsurancePlatform.ContractingService.Application.UseCases.CreateContract;

public sealed record CreateContractCommand(Guid ProposalId) : ICommand;
