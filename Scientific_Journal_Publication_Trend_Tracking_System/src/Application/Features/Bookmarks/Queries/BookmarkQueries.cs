using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Queries;

public record GetUserBookmarksQuery(
    Guid UserId,
    BookmarkType? Type = null) : IRequest<BookmarkListResponse>;

public record GetBookmarkByIdQuery(
    Guid UserId,
    Guid BookmarkId) : IRequest<BookmarkDto?>;
