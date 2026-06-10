using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using InsurancePlatform.ContractingService.Api.IntegrationTests.Infrastructure;

namespace InsurancePlatform.ContractingService.Api.IntegrationTests;

public sealed class ContractsApiTests : IClassFixture<ContractingApiFactory>
{
    private readonly ContractingApiFactory _factory;
    private readonly HttpClient _client;

    public ContractsApiTests(ContractingApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostContracts_ShouldCreateContractWhenProposalIsApproved()
    {
        var proposalId = Guid.NewGuid();
        _factory.AddProposal(proposalId, "Approved");

        var response = await CreateContractAsync(proposalId);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostContracts_ShouldReturnUnprocessableEntityWhenProposalIsRejected()
    {
        var proposalId = Guid.NewGuid();
        _factory.AddProposal(proposalId, "Rejected");

        var response = await CreateContractAsync(proposalId);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostContracts_ShouldReturnNotFoundWhenProposalDoesNotExist()
    {
        var proposalId = Guid.NewGuid();
        _factory.RemoveProposal(proposalId);

        var response = await CreateContractAsync(proposalId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostContracts_ShouldReturnConflictWhenContractAlreadyExistsForProposal()
    {
        var proposalId = Guid.NewGuid();
        _factory.AddProposal(proposalId, "Approved");

        var firstResponse = await CreateContractAsync(proposalId);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await CreateContractAsync(proposalId);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetContractById_ShouldReturnExistingContract()
    {
        var proposalId = Guid.NewGuid();
        _factory.AddProposal(proposalId, "Approved");
        var contractId = await CreateContractAndReadIdAsync(proposalId);

        var response = await _client.GetAsync($"/api/contracts/{contractId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var contract = await ReadJsonAsync(response);
        contract.RootElement.GetProperty("id").GetGuid().Should().Be(contractId);
        contract.RootElement.GetProperty("proposalId").GetGuid().Should().Be(proposalId);
    }

    [Fact]
    public async Task GetContractById_ShouldReturnNotFoundWhenContractDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/contracts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<HttpResponseMessage> CreateContractAsync(Guid proposalId)
    {
        var request = new
        {
            ProposalId = proposalId
        };

        return await _client.PostAsJsonAsync("/api/contracts", request);
    }

    private async Task<Guid> CreateContractAndReadIdAsync(Guid proposalId)
    {
        var response = await CreateContractAsync(proposalId);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var contract = await ReadJsonAsync(response);
        return contract.RootElement.GetProperty("id").GetGuid();
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
}
