using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Bookmarks.DTOs;

public record BookmarkDto
{
    public Guid Id { get; init; }
    public BookmarkType Type { get; init; }
    public Guid? ResearchPaperId { get; init; }
    public string? PaperTitle { get; init; }
    public Guid? KeywordId { get; init; }
    public string? KeywordName { get; init; }
    public Guid? JournalId { get; init; }
    public string? JournalTitle { get; init; }
    public Guid? ResearchTopicId { get; init; }
    public string? ResearchTopicName { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateBookmarkRequest
{
    public BookmarkType Type { get; init; }
    public Guid? ResearchPaperId { get; init; }
    public Guid? KeywordId { get; init; }
    public string? KeywordName { get; init; }
    public Guid? JournalId { get; init; }
    public Guid? ResearchTopicId { get; init; }
    public string? Notes { get; init; }
}

public record UpdateBookmarkRequest
{
    public string? Notes { get; init; }
}

public record BookmarkListResponse
{
    public List<BookmarkDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
}
