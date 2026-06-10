using InsurancePlatform.ContractingService.Application.Ports;
using InsurancePlatform.ContractingService.Domain.Repositories;
using InsurancePlatform.ContractingService.Infrastructure.Gateways;
using InsurancePlatform.ContractingService.Infrastructure.Persistence;
using InsurancePlatform.ContractingService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsurancePlatform.ContractingService.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddContractingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ContractingDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Contracting database connection string was not configured.");

        services.AddDbContext<ContractingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IContractRepository, ContractRepository>();

        var gatewayOptions = new ProposalServiceGatewayOptions
        {
            BaseUrl = configuration["ProposalServiceGateway:BaseUrl"]
                ?? configuration["ProposalService:BaseUrl"]
                ?? throw new InvalidOperationException("Proposal service base URL was not configured.")
        };

        services.AddSingleton(gatewayOptions);

        services.AddHttpClient<IProposalServiceGateway, ProposalServiceGateway>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<ProposalServiceGatewayOptions>();
            httpClient.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
        });

        return services;
    }
}
