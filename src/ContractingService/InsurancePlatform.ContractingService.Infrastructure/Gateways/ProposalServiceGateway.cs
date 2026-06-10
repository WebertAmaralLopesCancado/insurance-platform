using System.Net;
using System.Net.Http.Json;
using InsurancePlatform.ContractingService.Application.Ports;

namespace InsurancePlatform.ContractingService.Infrastructure.Gateways;

public sealed class ProposalServiceGateway : IProposalServiceGateway
{
    private readonly HttpClient _httpClient;

    public ProposalServiceGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProposalSnapshot?> GetProposalByIdAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"/api/proposals/{proposalId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var proposal = await response.Content.ReadFromJsonAsync<ProposalServiceResponse>(
            cancellationToken);

        return proposal is null
            ? null
            : new ProposalSnapshot(proposal.Id, proposal.Status);
    }

    private sealed record ProposalServiceResponse(Guid Id, string Status);
}
