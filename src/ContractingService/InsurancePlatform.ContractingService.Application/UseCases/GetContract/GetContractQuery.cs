using InsurancePlatform.ContractingService.Application.Common;

namespace InsurancePlatform.ContractingService.Application.UseCases.GetContract;

public sealed record GetContractQuery(Guid Id) : IQuery;
