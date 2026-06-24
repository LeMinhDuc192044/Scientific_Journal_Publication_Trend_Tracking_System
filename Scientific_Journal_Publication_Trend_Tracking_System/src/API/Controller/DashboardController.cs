using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.API.Controller;

[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get dashboard KPI summary statistics
    /// </summary>
    [HttpGet("summary")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardSummaryResponse>>> GetSummary(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);
        return Ok(ApiResponse<DashboardSummaryResponse>.Ok(result, "Dashboard summary retrieved successfully"));
    }

    /// <summary>
    /// Chart data: publications count grouped by year (line/bar chart)
    /// </summary>
    [HttpGet("charts/publications-by-year")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ChartResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChartResponse>>> GetPublicationsByYear(
        [FromQuery] int? fromYear,
        [FromQuery] int? toYear,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPublicationsByYearChartQuery(fromYear, toYear);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<ChartResponse>.Ok(result, "Publications by year chart data retrieved"));
    }

    /// <summary>
    /// Chart data: top keywords by frequency (bar/pie chart)
    /// </summary>
    [HttpGet("charts/top-keywords")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ChartResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChartResponse>>> GetTopKeywords(
        [FromQuery] int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTopKeywordsChartQuery(topCount);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<ChartResponse>.Ok(result, "Top keywords chart data retrieved"));
    }

    /// <summary>
    /// Chart data: publications grouped by research domain (pie chart)
    /// </summary>
    [HttpGet("charts/publications-by-domain")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ChartResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChartResponse>>> GetPublicationsByDomain(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPublicationsByDomainChartQuery(), cancellationToken);
        return Ok(ApiResponse<ChartResponse>.Ok(result, "Publications by domain chart data retrieved"));
    }

    /// <summary>
    /// Chart data: top journals by publication count (horizontal bar chart)
    /// </summary>
    [HttpGet("charts/top-journals")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ChartResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChartResponse>>> GetTopJournals(
        [FromQuery] int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTopJournalsChartQuery(topCount);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<ChartResponse>.Ok(result, "Top journals chart data retrieved"));
    }
}
