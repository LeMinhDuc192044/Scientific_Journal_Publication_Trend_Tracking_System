using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Helpers;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Exceptions;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Handlers;

public class CreateBookmarkCommandHandler : IRequestHandler<CreateBookmarkCommand, BookmarkDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookmarkCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BookmarkDto> Handle(CreateBookmarkCommand request, CancellationToken cancellationToken)
    {
        var keywordId = await ResolveKeywordIdAsync(request, cancellationToken);
        await ValidateTargetExistsAsync(request, keywordId, cancellationToken);

        var duplicateExists = await _unitOfWork.Bookmarks.ExistsAsync(
            b => b.UserId == request.UserId
                 && b.Type == request.Type
                 && b.ResearchPaperId == request.ResearchPaperId
                 && b.KeywordId == keywordId
                 && b.JournalId == request.JournalId
                 && b.ResearchTopicId == request.ResearchTopicId,
            cancellationToken);

        if (duplicateExists)
            throw new ConflictException("This item is already bookmarked.");

        var now = DateTime.UtcNow;
        var bookmark = new Bookmark
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type,
            ResearchPaperId = request.Type == BookmarkType.Paper ? request.ResearchPaperId : null,
            KeywordId = request.Type == BookmarkType.Keyword ? keywordId : null,
            JournalId = request.Type == BookmarkType.Journal ? request.JournalId : null,
            ResearchTopicId = request.Type == BookmarkType.ResearchTopic ? request.ResearchTopicId : null,
            Notes = request.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.Bookmarks.AddAsync(bookmark, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await BookmarkMapper.WithIncludes(_unitOfWork.Bookmarks.GetQueryable())
            .FirstAsync(b => b.Id == bookmark.Id, cancellationToken);

        return BookmarkMapper.ToDto(saved);
    }

    private async Task<Guid?> ResolveKeywordIdAsync(CreateBookmarkCommand request, CancellationToken cancellationToken)
    {
        if (request.Type != BookmarkType.Keyword)
            return null;

        if (request.KeywordId.HasValue)
            return request.KeywordId;

        var keywordName = request.KeywordName!.Trim();
        var existing = await _unitOfWork.Keywords
            .GetQueryable()
            .FirstOrDefaultAsync(k => k.Name.ToLower() == keywordName.ToLower(), cancellationToken);

        if (existing != null)
            return existing.Id;

        var now = DateTime.UtcNow;
        var keyword = new Keyword
        {
            Id = Guid.NewGuid(),
            Name = keywordName,
            FrequencyCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.Keywords.AddAsync(keyword, cancellationToken);
        return keyword.Id;
    }

    private async Task ValidateTargetExistsAsync(
        CreateBookmarkCommand request,
        Guid? keywordId,
        CancellationToken cancellationToken)
    {
        switch (request.Type)
        {
            case BookmarkType.Paper:
                if (!await _unitOfWork.ResearchPapers.ExistsAsync(p => p.Id == request.ResearchPaperId, cancellationToken))
                    throw new NotFoundException("Research paper not found.");
                break;
            case BookmarkType.Keyword:
                if (!await _unitOfWork.Keywords.ExistsAsync(k => k.Id == keywordId, cancellationToken))
                    throw new NotFoundException("Keyword not found.");
                break;
            case BookmarkType.Journal:
                if (!await _unitOfWork.Journals.ExistsAsync(j => j.Id == request.JournalId, cancellationToken))
                    throw new NotFoundException("Journal not found.");
                break;
            case BookmarkType.ResearchTopic:
                if (!await _unitOfWork.ResearchTopics.ExistsAsync(t => t.Id == request.ResearchTopicId, cancellationToken))
                    throw new NotFoundException("Research topic not found.");
                break;
        }
    }
}
