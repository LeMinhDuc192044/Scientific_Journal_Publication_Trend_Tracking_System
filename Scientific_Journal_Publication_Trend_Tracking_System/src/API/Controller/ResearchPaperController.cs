using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchPapers.Commands;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.API.Controller;

[ApiController]
[Route("api/research-papers")]
[Produces("application/json")]
public class ResearchPaperController : ControllerBase
{
    private readonly IMediator _mediator;

    public ResearchPaperController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("import-single")]
    [ProducesResponseType(typeof(ImportSinglePaperResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportSingle(
        [FromBody] ImportResearchPaperByLinkCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Search research papers with filters and pagination
    /// </summary>
    /// <param name="request">Search filters and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of research papers</returns>
    [HttpPost("search")]
    [Authorize(Roles = "Admin,User,Researcher")]
    [ProducesResponseType(typeof(ApiResponse<SearchPapersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SearchPapersResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SearchPapersResponse>>> SearchPapers(
        [FromBody] SearchPapersRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new SearchResearchPapersQuery(
                request.Keyword,
                request.Author,
                request.Journal,
                request.FromYear,
                request.ToYear,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDescending);

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(ApiResponse<SearchPapersResponse>.Ok(result, $"Found {result.TotalCount} papers"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SearchPapersResponse>.Failure($"Search failed: {ex.Message}"));
        }
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportPapersResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(
        [FromBody] ImportResearchPapersCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Imported == 0 && result.Errors.Count > 0)
            return StatusCode(StatusCodes.Status502BadGateway, result);

        return Ok(result);
    }

    /// <summary>
    /// Get research paper details by ID
    /// </summary>
    /// <param name="id">Research paper ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Research paper details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,User,Researcher")]
    [ProducesResponseType(typeof(ApiResponse<ResearchPaperDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ResearchPaperDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ResearchPaperDto>>> GetPaperDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetResearchPaperDetailQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
                return NotFound(ApiResponse<ResearchPaperDto>.Failure("Research paper not found", 404));

            return Ok(ApiResponse<ResearchPaperDto>.Ok(result, "Paper details retrieved successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ResearchPaperDto>.Failure($"Failed to retrieve paper: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get popular research papers (by citation count)
    /// </summary>
    /// <param name="topCount">Number of top papers to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of popular research papers</returns>
    [HttpGet("popular/{topCount?}")]
    [Authorize(Roles = "Admin,Researcher")]
    [ProducesResponseType(typeof(ApiResponse<List<ResearchPaperDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ResearchPaperDto>>>> GetPopularPapers(
        [FromRoute] int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (topCount <= 0 || topCount > 100)
                topCount = 10;

            var query = new GetPopularResearchPapersQuery(topCount);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(ApiResponse<List<ResearchPaperDto>>.Ok(result, $"Retrieved {result.Count} popular papers"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ResearchPaperDto>>.Failure($"Failed to retrieve popular papers: {ex.Message}"));
        }
    }

    /// <summary>
    /// Synchronize research papers from external academic APIs
    /// Requires Admin role
    /// </summary>
    /// <param name="command">Synchronization parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Synchronization result with counts</returns>
    [HttpPost("sync")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SyncPapersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SyncPapersResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SyncPapersResponse>>> SyncPapers(
        [FromBody] SyncResearchPapersCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Query))
                return BadRequest(ApiResponse<SyncPapersResponse>.Failure("Query cannot be empty"));

            if (string.IsNullOrWhiteSpace(command.ApiSource))
                return BadRequest(ApiResponse<SyncPapersResponse>.Failure("ApiSource must be specified (semanticscholar, openalex, or crossref)"));

            var validSources = new[] { "semanticscholar", "openalex", "crossref" };
            if (!validSources.Contains(command.ApiSource.ToLower()))
                return BadRequest(ApiResponse<SyncPapersResponse>.Failure($"Invalid ApiSource. Supported sources: {string.Join(", ", validSources)}"));

            var result = await _mediator.Send(command, cancellationToken);

            if (result.Errors.Count > 0)
                return Ok(ApiResponse<SyncPapersResponse>.Ok(result, $"Synchronization completed with errors. Created: {result.TotalCreated}, Updated: {result.TotalUpdated}"));

            return Ok(ApiResponse<SyncPapersResponse>.Ok(result, $"Synchronization completed successfully. Created: {result.TotalCreated}, Updated: {result.TotalUpdated}"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SyncPapersResponse>.Failure($"Synchronization failed: {ex.Message}"));
        }
    }
}
