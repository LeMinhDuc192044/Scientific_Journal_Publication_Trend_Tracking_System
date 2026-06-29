using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.API.Controller;

[ApiController]
[Authorize]
[Route("api/follow-subscriptions")]
[Produces("application/json")]
public sealed class FollowSubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowSubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FollowSubscriptionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<FollowSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Follow(
        [FromBody] CreateFollowSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateFollowSubscriptionCommand(request.TargetType, request.TargetId),
            cancellationToken);

        if (result.Created)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<FollowSubscriptionDto>.Created(
                    result.Subscription,
                    "Follow subscription created successfully"));
        }

        var message = result.Reactivated
            ? "Follow subscription reactivated successfully"
            : "Follow subscription is already active";

        return Ok(ApiResponse<FollowSubscriptionDto>.Ok(result.Subscription, message));
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FollowSubscriptionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(
        [FromQuery] FollowTargetType? targetType = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyFollowSubscriptionsQuery(targetType, pageNumber, pageSize),
            cancellationToken);

        return Ok(ApiResponse<PagedResult<FollowSubscriptionDto>>.Ok(
            result,
            "Follow subscriptions retrieved successfully"));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unfollow(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnfollowSubscriptionCommand(id), cancellationToken);
        return Ok(ApiResponse.Ok("Follow subscription deactivated successfully"));
    }
}
