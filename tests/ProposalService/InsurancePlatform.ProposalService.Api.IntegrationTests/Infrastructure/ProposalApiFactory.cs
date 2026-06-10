using InsurancePlatform.ProposalService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        builder.UseContentRoot(ResolveApiContentRoot());

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var configuration = new Dictionary<string, string?>
            {
                ["ConnectionStrings:ProposalDb"] = _postgresSqlContainer.ConnectionString
            };

            configurationBuilder.AddInMemoryCollection(configuration);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ProposalDbContext>>();

            services.AddDbContext<ProposalDbContext>(options =>
            {
                options.UseNpgsql(_postgresSqlContainer.ConnectionString);
            });
        });
    }

    private static string ResolveApiContentRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "insurance-platform.sln")))
        {
            directory = directory.Parent;
        }

        if (directory is null)
        {
            throw new DirectoryNotFoundException("Could not locate the solution root from the test base directory.");
        }

        return Path.Combine(
            directory.FullName,
            "src",
            "ProposalService",
            "InsurancePlatform.ProposalService.Api");
    }
}
