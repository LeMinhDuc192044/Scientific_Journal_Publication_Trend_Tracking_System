using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Authentication;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.API.Controller;

[ApiController]
[Route("api/bookmarks")]
[Produces("application/json")]
[Authorize]
public class BookmarkController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public BookmarkController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Save a bookmark for a paper, keyword, journal, or research topic
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BookmarkDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<BookmarkDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BookmarkDto>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBookmark(
        [FromBody] CreateBookmarkRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();

        var command = new CreateBookmarkCommand(
            userId,
            request.Type,
            request.ResearchPaperId,
            request.KeywordId,
            request.KeywordName,
            request.JournalId,
            request.ResearchTopicId,
            request.Notes);

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetBookmark), new { id = result.Id },
            ApiResponse<BookmarkDto>.Created(result, "Bookmark saved successfully"));
    }

    /// <summary>
    /// Get all bookmarks for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<BookmarkListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookmarks(
        [FromQuery] BookmarkType? type,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        var result = await _mediator.Send(new GetUserBookmarksQuery(userId, type), cancellationToken);
        return Ok(ApiResponse<BookmarkListResponse>.Ok(result, $"Retrieved {result.TotalCount} bookmarks"));
    }

    /// <summary>
    /// Get a bookmark by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<BookmarkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BookmarkDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookmark(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        var result = await _mediator.Send(new GetBookmarkByIdQuery(userId, id), cancellationToken);

        if (result == null)
            return NotFound(ApiResponse<BookmarkDto>.Failure("Bookmark not found", 404));

        return Ok(ApiResponse<BookmarkDto>.Ok(result));
    }

    /// <summary>
    /// Update bookmark notes
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<BookmarkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BookmarkDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBookmark(
        [FromRoute] Guid id,
        [FromBody] UpdateBookmarkRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        var result = await _mediator.Send(new UpdateBookmarkCommand(userId, id, request.Notes), cancellationToken);
        return Ok(ApiResponse<BookmarkDto>.Ok(result, "Bookmark updated successfully"));
    }

    /// <summary>
    /// Remove a bookmark (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBookmark(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetRequiredUserId();
        await _mediator.Send(new DeleteBookmarkCommand(userId, id), cancellationToken);
        return Ok(ApiResponse.Ok("Bookmark removed successfully"));
    }

    private Guid GetRequiredUserId()
    {
        return _currentUserService.GetUserId()
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }
}
