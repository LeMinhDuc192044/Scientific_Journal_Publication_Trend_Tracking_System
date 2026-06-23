using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Commands;

public record CreateBookmarkCommand(
    Guid UserId,
    BookmarkType Type,
    Guid? ResearchPaperId,
    Guid? KeywordId,
    string? KeywordName,
    Guid? JournalId,
    Guid? ResearchTopicId,
    string? Notes) : IRequest<BookmarkDto>;

public record UpdateBookmarkCommand(
    Guid UserId,
    Guid BookmarkId,
    string? Notes) : IRequest<BookmarkDto>;

public record DeleteBookmarkCommand(
    Guid UserId,
    Guid BookmarkId) : IRequest<bool>;
