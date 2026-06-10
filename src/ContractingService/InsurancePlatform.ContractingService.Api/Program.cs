using InsurancePlatform.ContractingService.Api.Middlewares;
using InsurancePlatform.ContractingService.Application.Common;
using InsurancePlatform.ContractingService.Application.UseCases.CreateContract;
using InsurancePlatform.ContractingService.Application.UseCases.GetContract;
using InsurancePlatform.ContractingService.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddContractingInfrastructure(builder.Configuration);

builder.Services.AddScoped<ICommandHandler<CreateContractCommand, CreateContractResponse>, CreateContractCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetContractQuery, ContractResponse>, GetContractQueryHandler>();

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
