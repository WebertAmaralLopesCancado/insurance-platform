using InsurancePlatform.ProposalService.Api.Requests;
using InsurancePlatform.ProposalService.Application.Common;
using InsurancePlatform.ProposalService.Application.UseCases.ApproveProposal;
using InsurancePlatform.ProposalService.Application.UseCases.CreateProposal;
using InsurancePlatform.ProposalService.Application.UseCases.GetAllProposals;
using InsurancePlatform.ProposalService.Application.UseCases.GetProposal;
using InsurancePlatform.ProposalService.Application.UseCases.RejectProposal;
using Microsoft.AspNetCore.Mvc;

namespace InsurancePlatform.ProposalService.Api.Controllers;

[ApiController]
[Route("api/proposals")]
public sealed class ProposalsController : ControllerBase
{
    private readonly ICommandHandler<CreateProposalCommand, CreateProposalResponse> _createProposalHandler;
    private readonly IQueryHandler<GetProposalQuery, ProposalResponse> _getProposalHandler;
    private readonly IQueryHandler<GetAllProposalsQuery, PagedResponse<ProposalListResponse>> _getAllProposalsHandler;
    private readonly ICommandHandler<ApproveProposalCommand, Result> _approveProposalHandler;
    private readonly ICommandHandler<RejectProposalCommand, Result> _rejectProposalHandler;

    public ProposalsController(
        ICommandHandler<CreateProposalCommand, CreateProposalResponse> createProposalHandler,
        IQueryHandler<GetProposalQuery, ProposalResponse> getProposalHandler,
        IQueryHandler<GetAllProposalsQuery, PagedResponse<ProposalListResponse>> getAllProposalsHandler,
        ICommandHandler<ApproveProposalCommand, Result> approveProposalHandler,
        ICommandHandler<RejectProposalCommand, Result> rejectProposalHandler)
    {
        _createProposalHandler = createProposalHandler;
        _getProposalHandler = getProposalHandler;
        _getAllProposalsHandler = getAllProposalsHandler;
        _approveProposalHandler = approveProposalHandler;
        _rejectProposalHandler = rejectProposalHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateProposalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProposalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProposalCommand(
            request.CustomerName,
            request.InsuranceType,
            request.CoverageAmount);

        var response = await _createProposalHandler.HandleAsync(command, cancellationToken);

        return Created($"/api/proposals/{response.Id}", response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProposalResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _getProposalHandler.HandleAsync(
            new GetProposalQuery(id),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProposalListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProposalListResponse>>> GetAllAsync(
        [FromQuery] PaginationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _getAllProposalsHandler.HandleAsync(
            new GetAllProposalsQuery(request.PageNumber, request.PageSize),
            cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApproveAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _approveProposalHandler.HandleAsync(
            new ApproveProposalCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RejectAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _rejectProposalHandler.HandleAsync(
            new RejectProposalCommand(id),
            cancellationToken);

        return NoContent();
    }
}
