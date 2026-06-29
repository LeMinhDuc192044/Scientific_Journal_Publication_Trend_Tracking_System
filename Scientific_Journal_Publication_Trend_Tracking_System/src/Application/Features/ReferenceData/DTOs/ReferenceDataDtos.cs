using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.DTOs;

public sealed record JournalListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Issn { get; init; }
    public string? Publisher { get; init; }
    public int TotalPapersPublished { get; init; }
}

public sealed record ResearchTopicListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ResearchDomain Domain { get; init; }
    public int PapersCount { get; init; }
}
