using System.Collections.Concurrent;
using InsurancePlatform.ContractingService.Application.Ports;
using InsurancePlatform.ContractingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InsurancePlatform.ContractingService.Api.IntegrationTests.Infrastructure;

public sealed class ContractingApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _postgresSqlContainer = new();
    private readonly ConcurrentDictionary<Guid, ProposalSnapshot> _proposals = new();

    public void AddProposal(Guid proposalId, string status)
    {
        _proposals[proposalId] = new ProposalSnapshot(proposalId, status);
    }

    public void RemoveProposal(Guid proposalId)
    {
        _proposals.TryRemove(proposalId, out _);
    }

    public async Task InitializeAsync()
    {
        await _postgresSqlContainer.InitializeAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContractingDbContext>();
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
                ["ConnectionStrings:ContractingDb"] = _postgresSqlContainer.ConnectionString,
                ["ProposalServiceGateway:BaseUrl"] = "http://proposal-service.test"
            };

            configurationBuilder.AddInMemoryCollection(configuration);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ContractingDbContext>>();
            services.AddDbContext<ContractingDbContext>(options =>
            {
                options.UseNpgsql(_postgresSqlContainer.ConnectionString);
            });

            services.RemoveAll<IProposalServiceGateway>();
            services.AddSingleton<IProposalServiceGateway>(new StubProposalServiceGateway(_proposals));
        });
    }

    private sealed class StubProposalServiceGateway : IProposalServiceGateway
    {
        private readonly ConcurrentDictionary<Guid, ProposalSnapshot> _proposals;

        public StubProposalServiceGateway(ConcurrentDictionary<Guid, ProposalSnapshot> proposals)
        {
            _proposals = proposals;
        }

        public Task<ProposalSnapshot?> GetProposalByIdAsync(
            Guid proposalId,
            CancellationToken cancellationToken = default)
        {
            _proposals.TryGetValue(proposalId, out var proposal);
            return Task.FromResult(proposal);
        }
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
            "ContractingService",
            "InsurancePlatform.ContractingService.Api");
    }
}
