using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.Helpers;

internal static class BookmarkMapper
{
    public static BookmarkDto ToDto(Bookmark bookmark)
    {
        return new BookmarkDto
        {
            Id = bookmark.Id,
            Type = bookmark.Type,
            ResearchPaperId = bookmark.ResearchPaperId,
            PaperTitle = bookmark.ResearchPaper?.Title,
            KeywordId = bookmark.KeywordId,
            KeywordName = bookmark.Keyword?.Name,
            JournalId = bookmark.JournalId,
            JournalTitle = bookmark.Journal?.Title,
            ResearchTopicId = bookmark.ResearchTopicId,
            ResearchTopicName = bookmark.ResearchTopic?.Name,
            Notes = bookmark.Notes,
            CreatedAt = bookmark.CreatedAt
        };
    }

    public static IQueryable<Bookmark> WithIncludes(IQueryable<Bookmark> query)
    {
        return query
            .Include(b => b.ResearchPaper)
            .Include(b => b.Keyword)
            .Include(b => b.Journal)
            .Include(b => b.ResearchTopic);
    }
}
