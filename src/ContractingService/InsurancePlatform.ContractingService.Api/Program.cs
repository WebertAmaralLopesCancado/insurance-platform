using InsurancePlatform.ContractingService.Api.Middlewares;
using InsurancePlatform.ContractingService.Application.DependencyInjection;
using InsurancePlatform.ContractingService.Infrastructure.DependencyInjection;
using InsurancePlatform.ContractingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddContractingApplication();
builder.Services.AddContractingInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ContractingDbContext>();
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
