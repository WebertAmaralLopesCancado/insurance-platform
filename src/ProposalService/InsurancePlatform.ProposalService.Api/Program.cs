using InsurancePlatform.ProposalService.Api.Middlewares;
using InsurancePlatform.ProposalService.Application.DependencyInjection;
using InsurancePlatform.ProposalService.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddProposalApplication();
builder.Services.AddProposalInfrastructure(builder.Configuration);

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
