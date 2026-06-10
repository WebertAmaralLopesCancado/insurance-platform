using InsurancePlatform.ProposalService.Api.Middlewares;
using InsurancePlatform.ProposalService.Application.Common;
using InsurancePlatform.ProposalService.Application.UseCases.ApproveProposal;
using InsurancePlatform.ProposalService.Application.UseCases.CreateProposal;
using InsurancePlatform.ProposalService.Application.UseCases.GetAllProposals;
using InsurancePlatform.ProposalService.Application.UseCases.GetProposal;
using InsurancePlatform.ProposalService.Application.UseCases.RejectProposal;
using InsurancePlatform.ProposalService.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddProposalInfrastructure(builder.Configuration);

builder.Services.AddScoped<ICommandHandler<CreateProposalCommand, CreateProposalResponse>, CreateProposalCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetProposalQuery, ProposalResponse>, GetProposalQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetAllProposalsQuery, PagedResponse<ProposalListResponse>>, GetAllProposalsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<ApproveProposalCommand, Result>, ApproveProposalCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RejectProposalCommand, Result>, RejectProposalCommandHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
