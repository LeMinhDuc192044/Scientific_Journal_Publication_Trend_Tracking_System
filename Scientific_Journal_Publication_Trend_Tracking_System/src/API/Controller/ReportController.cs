using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.API.Controller;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get live analytical summary without saving
    /// </summary>
    [HttpGet("summary")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AnalyticalReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAnalyticalReportSummaryQuery(), cancellationToken);
        return Ok(ApiResponse<AnalyticalReportDto>.Ok(result, "Analytical summary generated"));
    }

    /// <summary>
    /// Generate and save an analytical report
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(ApiResponse<AnalyticalReportDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> GenerateReport(
        [FromBody] GenerateReportRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GenerateAnalyticalReportCommand(request?.Title),
            cancellationToken);

        return CreatedAtAction(nameof(GetReport), new { id = result.Id },
            ApiResponse<AnalyticalReportDto>.Created(result, "Analytical report generated successfully"));
    }

    /// <summary>
    /// List saved analytical reports
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ReportListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReports(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetReportsQuery(pageNumber, pageSize), cancellationToken);
        return Ok(ApiResponse<ReportListResponse>.Ok(result, $"Retrieved {result.Items.Count} reports"));
    }

    /// <summary>
    /// Get a saved analytical report by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AnalyticalReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AnalyticalReportDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportByIdQuery(id), cancellationToken);

        if (result == null)
            return NotFound(ApiResponse<AnalyticalReportDto>.Failure("Report not found", 404));

        return Ok(ApiResponse<AnalyticalReportDto>.Ok(result));
    }
}
