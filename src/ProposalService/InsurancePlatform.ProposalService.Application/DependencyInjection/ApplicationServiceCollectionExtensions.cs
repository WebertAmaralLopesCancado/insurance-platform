using InsurancePlatform.ProposalService.Application.Common;
using InsurancePlatform.ProposalService.Application.UseCases.ApproveProposal;
using InsurancePlatform.ProposalService.Application.UseCases.CreateProposal;
using InsurancePlatform.ProposalService.Application.UseCases.GetAllProposals;
using InsurancePlatform.ProposalService.Application.UseCases.GetProposal;
using InsurancePlatform.ProposalService.Application.UseCases.RejectProposal;
using Microsoft.Extensions.DependencyInjection;

namespace InsurancePlatform.ProposalService.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddProposalApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateProposalCommand, CreateProposalResponse>, CreateProposalCommandHandler>();
        services.AddScoped<IQueryHandler<GetProposalQuery, ProposalResponse>, GetProposalQueryHandler>();
        services.AddScoped<IQueryHandler<GetAllProposalsQuery, PagedResponse<ProposalListResponse>>, GetAllProposalsQueryHandler>();
        services.AddScoped<ICommandHandler<ApproveProposalCommand, Result>, ApproveProposalCommandHandler>();
        services.AddScoped<ICommandHandler<RejectProposalCommand, Result>, RejectProposalCommandHandler>();

        return services;
    }
}
