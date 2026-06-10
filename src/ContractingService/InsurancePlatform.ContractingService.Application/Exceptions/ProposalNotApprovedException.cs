namespace InsurancePlatform.ContractingService.Application.Exceptions;

public class ProposalNotApprovedException : Exception
{
    public ProposalNotApprovedException(string message)
        : base(message)
    {
    }

    public ProposalNotApprovedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
