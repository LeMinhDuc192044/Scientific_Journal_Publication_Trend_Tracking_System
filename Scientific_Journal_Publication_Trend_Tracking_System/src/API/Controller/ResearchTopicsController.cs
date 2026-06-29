using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.API.Controller;

[ApiController]
[Route("api/research-topics")]
[Produces("application/json")]
public sealed class ResearchTopicsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResearchTopicsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ResearchTopicListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetResearchTopicsQuery(search, pageNumber, pageSize),
            cancellationToken);

        return Ok(ApiResponse<PagedResult<ResearchTopicListItemDto>>.Ok(
            result,
            "Research topics retrieved successfully"));
    }
}
