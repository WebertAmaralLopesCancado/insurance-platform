using InsurancePlatform.ContractingService.Api.Requests;
using InsurancePlatform.ContractingService.Application.Common;
using InsurancePlatform.ContractingService.Application.UseCases.CreateContract;
using InsurancePlatform.ContractingService.Application.UseCases.GetContract;
using Microsoft.AspNetCore.Mvc;

namespace InsurancePlatform.ContractingService.Api.Controllers;

[ApiController]
[Route("api/contracts")]
public sealed class ContractsController : ControllerBase
{
    private readonly ICommandHandler<CreateContractCommand, CreateContractResponse> _createContractHandler;
    private readonly IQueryHandler<GetContractQuery, ContractResponse> _getContractHandler;

    public ContractsController(
        ICommandHandler<CreateContractCommand, CreateContractResponse> createContractHandler,
        IQueryHandler<GetContractQuery, ContractResponse> getContractHandler)
    {
        _createContractHandler = createContractHandler;
        _getContractHandler = getContractHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateContractResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _createContractHandler.HandleAsync(
            new CreateContractCommand(request.ProposalId),
            cancellationToken);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _getContractHandler.HandleAsync(
            new GetContractQuery(id),
            cancellationToken);

        return Ok(response);
    }
}
