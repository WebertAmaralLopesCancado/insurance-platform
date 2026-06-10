namespace InsurancePlatform.ProposalService.Api.Requests;

public sealed class PaginationRequest
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
