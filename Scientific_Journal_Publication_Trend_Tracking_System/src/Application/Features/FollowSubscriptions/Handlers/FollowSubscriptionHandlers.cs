using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Authentication;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.FollowSubscriptions.Handlers;

public sealed class CreateFollowSubscriptionCommandHandler
    : IRequestHandler<CreateFollowSubscriptionCommand, CreateFollowSubscriptionResult>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateFollowSubscriptionCommandHandler(
        AppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<CreateFollowSubscriptionResult> Handle(
        CreateFollowSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var targetName = await GetTargetNameAsync(request, cancellationToken);

        var existing = await _dbContext.FollowSubscriptions
            .SingleOrDefaultAsync(
                f => f.UserId == userId &&
                     (request.TargetType == FollowTargetType.Journal
                         ? f.JournalId == request.TargetId
                         : f.ResearchTopicId == request.TargetId),
                cancellationToken);

        if (existing is not null)
        {
            var reactivated = !existing.IsActive;
            if (reactivated)
            {
                existing.IsActive = true;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return new CreateFollowSubscriptionResult(
                ToDto(existing, targetName),
                Created: false,
                Reactivated: reactivated);
        }

        var subscription = new FollowSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = request.TargetType,
            JournalId = request.TargetType == FollowTargetType.Journal ? request.TargetId : null,
            ResearchTopicId = request.TargetType == FollowTargetType.ResearchTopic ? request.TargetId : null,
            IsActive = true
        };

        await _dbContext.FollowSubscriptions.AddAsync(subscription, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateFollowSubscriptionResult(
            ToDto(subscription, targetName),
            Created: true,
            Reactivated: false);
    }

    private async Task<string> GetTargetNameAsync(
        CreateFollowSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        string? targetName;

        if (request.TargetType == FollowTargetType.Journal)
        {
            targetName = await _dbContext.Journals
                .AsNoTracking()
                .Where(j => j.Id == request.TargetId)
                .Select(j => j.Title)
                .SingleOrDefaultAsync(cancellationToken);
        }
        else
        {
            targetName = await _dbContext.ResearchTopics
                .AsNoTracking()
                .Where(t => t.Id == request.TargetId)
                .Select(t => t.Name)
                .SingleOrDefaultAsync(cancellationToken);
        }

        return targetName ?? throw new NotFoundException(
            $"{request.TargetType} '{request.TargetId}' was not found.");
    }

    private Guid GetCurrentUserId() =>
        _currentUserService.GetUserId()
        ?? throw new UnauthorizedException("User is not authenticated.");

    private static FollowSubscriptionDto ToDto(
        FollowSubscription subscription,
        string targetName) => new()
    {
        Id = subscription.Id,
        TargetType = subscription.Type,
        TargetId = subscription.JournalId ?? subscription.ResearchTopicId!.Value,
        TargetName = targetName,
        IsActive = subscription.IsActive,
        CreatedAt = subscription.CreatedAt,
        UpdatedAt = subscription.UpdatedAt
    };
}

public sealed class UnfollowSubscriptionCommandHandler
    : IRequestHandler<UnfollowSubscriptionCommand>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UnfollowSubscriptionCommandHandler(
        AppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task Handle(
        UnfollowSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId()
            ?? throw new UnauthorizedException("User is not authenticated.");

        var subscription = await _dbContext.FollowSubscriptions
            .SingleOrDefaultAsync(
                f => f.Id == request.SubscriptionId && f.UserId == userId,
                cancellationToken);

        if (subscription is null)
            throw new NotFoundException("Follow subscription was not found.");

        if (!subscription.IsActive)
            return;

        subscription.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class GetMyFollowSubscriptionsQueryHandler
    : IRequestHandler<GetMyFollowSubscriptionsQuery, PagedResult<FollowSubscriptionDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyFollowSubscriptionsQueryHandler(
        AppDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<FollowSubscriptionDto>> Handle(
        GetMyFollowSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId()
            ?? throw new UnauthorizedException("User is not authenticated.");

        var query = _dbContext.FollowSubscriptions
            .AsNoTracking()
            .Where(f => f.UserId == userId && f.IsActive);

        if (request.TargetType.HasValue)
            query = query.Where(f => f.Type == request.TargetType.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(f => f.UpdatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FollowSubscriptionDto
            {
                Id = f.Id,
                TargetType = f.Type,
                TargetId = f.JournalId ?? f.ResearchTopicId!.Value,
                TargetName = f.Type == FollowTargetType.Journal
                    ? f.Journal!.Title
                    : f.ResearchTopic!.Name,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<FollowSubscriptionDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
