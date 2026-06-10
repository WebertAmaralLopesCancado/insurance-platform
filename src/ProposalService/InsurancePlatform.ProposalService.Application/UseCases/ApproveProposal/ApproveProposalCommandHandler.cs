using InsurancePlatform.ProposalService.Application.Common;
using InsurancePlatform.ProposalService.Application.Exceptions;
using InsurancePlatform.ProposalService.Domain.Repositories;

namespace InsurancePlatform.ProposalService.Application.UseCases.ApproveProposal;

public sealed class ApproveProposalCommandHandler
    : ICommandHandler<ApproveProposalCommand, Result>
{
    private readonly IProposalRepository _proposalRepository;

    public ApproveProposalCommandHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<Result> HandleAsync(
        ApproveProposalCommand command,
        CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.Id, cancellationToken);

        if (proposal is null)
        {
            throw new NotFoundException($"Proposal with id '{command.Id}' was not found.");
        }

        proposal.Approve();

        await _proposalRepository.UpdateAsync(proposal, cancellationToken);

        return Result.Success();
    }
}
