using InsurancePlatform.ProposalService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsurancePlatform.ProposalService.Api.IntegrationTests.Infrastructure;

public sealed class ProposalApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _postgresSqlContainer = new();

    public async Task InitializeAsync()
    {
        await _postgresSqlContainer.InitializeAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProposalDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var configuration = new Dictionary<string, string?>
            {
                ["ConnectionStrings:ProposalDb"] = _postgresSqlContainer.ConnectionString
            };

            configurationBuilder.AddInMemoryCollection(configuration);
        });
    }
}
