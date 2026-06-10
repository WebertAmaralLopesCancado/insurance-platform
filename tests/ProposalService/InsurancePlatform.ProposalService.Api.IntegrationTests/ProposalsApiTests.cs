using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using InsurancePlatform.ProposalService.Api.IntegrationTests.Infrastructure;

namespace InsurancePlatform.ProposalService.Api.IntegrationTests;

public sealed class ProposalsApiTests : IClassFixture<ProposalApiFactory>
{
    private readonly HttpClient _client;

    public ProposalsApiTests(ProposalApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task PostProposals_ShouldCreateProposal()
    {
        var response = await CreateProposalAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task GetProposalById_ShouldReturnCreatedProposal()
    {
        var proposalId = await CreateProposalAndReadIdAsync();

        var response = await _client.GetAsync($"/api/proposals/{proposalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var proposal = await ReadJsonAsync(response);
        proposal.RootElement.GetProperty("id").GetGuid().Should().Be(proposalId);
        proposal.RootElement.GetProperty("status").GetString().Should().Be("UnderAnalysis");
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task PatchApproveProposal_ShouldApproveProposal()
    {
        var proposalId = await CreateProposalAndReadIdAsync();

        var response = await _client.PatchAsync($"/api/proposals/{proposalId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task PatchRejectProposal_ShouldRejectProposal()
    {
        var proposalId = await CreateProposalAndReadIdAsync();

        var response = await _client.PatchAsync($"/api/proposals/{proposalId}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task GetProposalById_ShouldReturnNotFoundWhenProposalDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/proposals/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<HttpResponseMessage> CreateProposalAsync()
    {
        var request = new
        {
            CustomerName = "John Doe",
            InsuranceType = "Auto",
            CoverageAmount = 15000m
        };

        return await _client.PostAsJsonAsync("/api/proposals", request);
    }

    private async Task<Guid> CreateProposalAndReadIdAsync()
    {
        var response = await CreateProposalAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var proposal = await ReadJsonAsync(response);
        return proposal.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
}
