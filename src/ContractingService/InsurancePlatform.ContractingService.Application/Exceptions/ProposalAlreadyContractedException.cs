namespace InsurancePlatform.ContractingService.Application.Exceptions;

public class ProposalAlreadyContractedException : ConflictException
{
    public ProposalAlreadyContractedException(string message)
        : base(message)
    {
    }

    public ProposalAlreadyContractedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
