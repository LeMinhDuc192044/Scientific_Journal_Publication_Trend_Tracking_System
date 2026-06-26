using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.DTO;

namespace Scientific_Journal_Publication_Trend_Tracking_System.API.Controllers;

[ApiController]
[Route("api/research-topics")]
[Produces("application/json")]
public class ResearchTopicsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResearchTopicsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Creates a new research topic.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResearchTopicDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateResearchTopicCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Returns a single research topic by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResearchTopicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetResearchTopicByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Returns all research topics, ordered by name.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ResearchTopicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetResearchTopicsListCommand(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Searches research topics by partial name match, paginated.</summary>
    /// <remarks>GET /api/research-topics/search?name=machine&amp;pageNumber=1&amp;pageSize=20</remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedList<ResearchTopicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search(
        [FromQuery] string name,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new SearchResearchTopicsByNameQuery(name, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Updates an existing research topic.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ResearchTopicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] ResearchTopicDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateResearchTopicCommand(id, request.Name, request.Description, request.Domain);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Deletes a research topic.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteResearchTopicCommand(id), cancellationToken);
        return NoContent();
    }
}