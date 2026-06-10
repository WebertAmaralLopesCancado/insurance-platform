using InsurancePlatform.ProposalService.Domain.Repositories;
using InsurancePlatform.ProposalService.Infrastructure.Persistence;
using InsurancePlatform.ProposalService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsurancePlatform.ProposalService.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddProposalInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ProposalDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Proposal database connection string was not configured.");

        services.AddDbContext<ProposalDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IProposalRepository, ProposalRepository>();

        return services;
    }
}
