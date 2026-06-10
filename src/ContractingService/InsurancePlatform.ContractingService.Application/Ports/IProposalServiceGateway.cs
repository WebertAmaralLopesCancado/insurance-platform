namespace InsurancePlatform.ContractingService.Application.Ports;

public interface IProposalServiceGateway
{
    Task<ProposalSnapshot?> GetProposalByIdAsync(
        Guid proposalId,
        CancellationToken cancellationToken = default);
}
