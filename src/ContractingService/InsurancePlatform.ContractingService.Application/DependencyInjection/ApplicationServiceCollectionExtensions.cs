using InsurancePlatform.ContractingService.Application.Common;
using InsurancePlatform.ContractingService.Application.UseCases.CreateContract;
using InsurancePlatform.ContractingService.Application.UseCases.GetContract;
using Microsoft.Extensions.DependencyInjection;

namespace InsurancePlatform.ContractingService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddContractingApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateContractCommand, CreateContractResponse>, CreateContractCommandHandler>();
        services.AddScoped<IQueryHandler<GetContractQuery, ContractResponse>, GetContractQueryHandler>();

        return services;
    }
}
