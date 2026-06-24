using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Helpers;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Handlers;

public class GetUserBookmarksQueryHandler : IRequestHandler<GetUserBookmarksQuery, BookmarkListResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserBookmarksQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BookmarkListResponse> Handle(GetUserBookmarksQuery request, CancellationToken cancellationToken)
    {
        var query = BookmarkMapper.WithIncludes(_unitOfWork.Bookmarks.GetQueryable())
            .Where(b => b.UserId == request.UserId);

        if (request.Type.HasValue)
            query = query.Where(b => b.Type == request.Type.Value);

        var bookmarks = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return new BookmarkListResponse
        {
            Items = bookmarks.Select(BookmarkMapper.ToDto).ToList(),
            TotalCount = bookmarks.Count
        };
    }
}

public class GetBookmarkByIdQueryHandler : IRequestHandler<GetBookmarkByIdQuery, BookmarkDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBookmarkByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BookmarkDto?> Handle(GetBookmarkByIdQuery request, CancellationToken cancellationToken)
    {
        var bookmark = await BookmarkMapper.WithIncludes(_unitOfWork.Bookmarks.GetQueryable())
            .FirstOrDefaultAsync(b => b.Id == request.BookmarkId && b.UserId == request.UserId, cancellationToken);

        return bookmark == null ? null : BookmarkMapper.ToDto(bookmark);
    }
}
