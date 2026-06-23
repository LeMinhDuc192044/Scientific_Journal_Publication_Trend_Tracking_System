namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.DTOs;

public record DomainStatDto
{
    public string Domain { get; init; } = string.Empty;
    public int PaperCount { get; init; }
}

public record YearlyPublicationDto
{
    public int Year { get; init; }
    public int Count { get; init; }
}

public record KeywordStatDto
{
    public string Name { get; init; } = string.Empty;
    public int Frequency { get; init; }
}

public record CitedPaperDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int CitationCount { get; init; }
    public int PublicationYear { get; init; }
}

public record AnalyticalReportDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int TotalPapersCount { get; init; }
    public int ActiveUsersCount { get; init; }
    public int TotalBookmarksCount { get; init; }
    public List<DomainStatDto> TopResearchDomains { get; init; } = new();
    public List<CitedPaperDto> MostCitedPapers { get; init; } = new();
    public List<YearlyPublicationDto> PublicationsByYear { get; init; } = new();
    public List<KeywordStatDto> TopKeywords { get; init; } = new();
    public DateTime GeneratedAt { get; init; }
}

public record GenerateReportRequest
{
    public string? Title { get; init; }
}

public record ReportListResponse
{
    public List<AnalyticalReportDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
}
