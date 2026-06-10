namespace InsurancePlatform.ContractingService.Application.Common;

public sealed class Result
{
    private Result(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public bool IsSuccess { get; }

    public static Result Success()
    {
        return new Result(true);
    }
}
