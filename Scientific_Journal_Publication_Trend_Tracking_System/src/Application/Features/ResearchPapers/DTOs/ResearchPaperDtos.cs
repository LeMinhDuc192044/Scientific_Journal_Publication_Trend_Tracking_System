using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;

/// <summary>
/// DTO for research paper details
/// </summary>
public record ResearchPaperDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Abstract { get; init; }
    public int PublicationYear { get; init; }
    public string? Doi { get; init; }
    public int CitationCount { get; init; }
    public string[] Keywords { get; init; } = Array.Empty<string>();
    public ResearchDomain Domain { get; init; }
    public JournalDto? Journal { get; init; }
    public List<AuthorDto> Authors { get; init; } = new();
    public List<ResearchTopicDto> Topics { get; init; } = new();
}

/// <summary>
/// DTO for journal information
/// </summary>
public record JournalDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Issn { get; init; }
    public string? Publisher { get; init; }
}

/// <summary>
/// DTO for author information
/// </summary>
public record AuthorDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Orcid { get; init; }
}

/// <summary>
/// DTO for a research topic linked to a paper.
/// </summary>
public record ResearchTopicDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ResearchDomain Domain { get; init; }
}

/// <summary>
/// DTO for research paper search filters
/// </summary>
public record SearchPapersRequest
{
    public string? Keyword { get; init; }
    public string? Author { get; init; }
    public string? Journal { get; init; }
    public int? FromYear { get; init; }
    public int? ToYear { get; init; }
    public ResearchDomain? Domain { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "PublicationYear";
    public bool SortDescending { get; init; } = true;
}
