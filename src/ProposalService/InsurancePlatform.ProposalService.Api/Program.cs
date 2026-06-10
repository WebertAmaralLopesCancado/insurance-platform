using InsurancePlatform.ProposalService.Api.Middlewares;
using InsurancePlatform.ProposalService.Application.DependencyInjection;
using InsurancePlatform.ProposalService.Infrastructure.DependencyInjection;
using InsurancePlatform.ProposalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddProposalApplication();
builder.Services.AddProposalInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ProposalDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program
{
}
