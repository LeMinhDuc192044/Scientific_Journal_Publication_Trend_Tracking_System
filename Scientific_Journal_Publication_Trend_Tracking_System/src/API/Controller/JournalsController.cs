using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.API.Controller;

[ApiController]
[Route("api/journals")]
[Produces("application/json")]
public sealed class JournalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JournalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<JournalListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetJournalsQuery(search, pageNumber, pageSize),
            cancellationToken);

        return Ok(ApiResponse<PagedResult<JournalListItemDto>>.Ok(
            result,
            "Journals retrieved successfully"));
    }
}
