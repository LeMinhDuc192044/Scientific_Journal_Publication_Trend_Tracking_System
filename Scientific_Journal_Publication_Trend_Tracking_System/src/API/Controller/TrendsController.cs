using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.API.Controller;

[ApiController]
[Route("api/trends")]
[Produces("application/json")]
public class TrendsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrendsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Track publication trends by keyword over time
    /// </summary>
    [HttpGet("keywords")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<KeywordTrendResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<KeywordTrendResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<KeywordTrendResponse>>> GetKeywordTrend(
        [FromQuery] string keyword,
        [FromQuery] int? fromYear,
        [FromQuery] int? toYear,
        CancellationToken cancellationToken = default)
    {
        var query = new GetKeywordTrendQuery(keyword, fromYear, toYear);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<KeywordTrendResponse>.Ok(
            result,
            $"Trend data for keyword '{result.Keyword}' retrieved successfully"));
    }

    /// <summary>
    /// List all available research topics
    /// </summary>
    [HttpGet("topics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<ResearchTopicListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ResearchTopicListItemDto>>>> GetResearchTopics(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetResearchTopicsQuery(), cancellationToken);
        return Ok(ApiResponse<List<ResearchTopicListItemDto>>.Ok(
            result,
            $"Retrieved {result.Count} research topics"));
    }

    /// <summary>
    /// Track publication trends by research topic over time
    /// </summary>
    [HttpGet("topics/{topicId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TopicTrendResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TopicTrendResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TopicTrendResponse>>> GetTopicTrend(
        [FromRoute] Guid topicId,
        [FromQuery] int? fromYear,
        [FromQuery] int? toYear,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTopicTrendQuery(topicId, fromYear, toYear);
        var result = await _mediator.Send(query, cancellationToken);

        if (result is null)
            return NotFound(ApiResponse<TopicTrendResponse>.Failure("Research topic not found", 404));

        return Ok(ApiResponse<TopicTrendResponse>.Ok(
            result,
            $"Trend data for topic '{result.TopicName}' retrieved successfully"));
    }

    /// <summary>
    /// View trending research topics and keywords ranked by publication growth
    /// </summary>
    [HttpGet("trending-topics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TrendingTopicsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TrendingTopicsResponse>>> GetTrendingTopics(
        [FromQuery] int topCount = 10,
        [FromQuery] int recentYears = 2,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTrendingTopicsQuery(topCount, recentYears);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ApiResponse<TrendingTopicsResponse>.Ok(
            result,
            $"Retrieved {result.Items.Count} trending topics"));
    }
}
