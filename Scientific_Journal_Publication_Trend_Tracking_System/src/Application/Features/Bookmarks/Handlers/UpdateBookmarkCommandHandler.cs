using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Helpers;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Handlers;

public class UpdateBookmarkCommandHandler : IRequestHandler<UpdateBookmarkCommand, BookmarkDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBookmarkCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BookmarkDto> Handle(UpdateBookmarkCommand request, CancellationToken cancellationToken)
    {
        var bookmark = await _unitOfWork.Bookmarks.GetByIdAsync(request.BookmarkId, cancellationToken);

        if (bookmark == null || bookmark.UserId != request.UserId)
            throw new NotFoundException("Bookmark not found.");

        bookmark.Notes = request.Notes;
        bookmark.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Bookmarks.Update(bookmark);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await BookmarkMapper.WithIncludes(_unitOfWork.Bookmarks.GetQueryable())
            .FirstAsync(b => b.Id == bookmark.Id, cancellationToken);

        return BookmarkMapper.ToDto(updated);
    }
}

public class DeleteBookmarkCommandHandler : IRequestHandler<DeleteBookmarkCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBookmarkCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteBookmarkCommand request, CancellationToken cancellationToken)
    {
        var bookmark = await _unitOfWork.Bookmarks.GetByIdAsync(request.BookmarkId, cancellationToken);

        if (bookmark == null || bookmark.UserId != request.UserId)
            throw new NotFoundException("Bookmark not found.");

        bookmark.IsDeleted = true;
        bookmark.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Bookmarks.Update(bookmark);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
