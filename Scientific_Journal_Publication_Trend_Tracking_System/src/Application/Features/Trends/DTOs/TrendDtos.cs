namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.DTOs;

public record TrendDataPointDto
{
    public int Year { get; init; }
    public int PublicationCount { get; init; }
    public double AverageCitations { get; init; }
    public double GrowthRate { get; init; }
}

public record KeywordTrendResponse
{
    public string Keyword { get; init; } = string.Empty;
    public int FromYear { get; init; }
    public int ToYear { get; init; }
    public List<TrendDataPointDto> DataPoints { get; init; } = new();
    public int TotalPublications { get; init; }
    public double OverallGrowthRate { get; init; }
}

public record TopicTrendResponse
{
    public Guid TopicId { get; init; }
    public string TopicName { get; init; } = string.Empty;
    public int FromYear { get; init; }
    public int ToYear { get; init; }
    public List<TrendDataPointDto> DataPoints { get; init; } = new();
    public int TotalPublications { get; init; }
    public double OverallGrowthRate { get; init; }
}

public record TrendingTopicItemDto
{
    public int Rank { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int RecentCount { get; init; }
    public int PreviousCount { get; init; }
    public double GrowthRate { get; init; }
    public int TotalPapers { get; init; }
}

public record TrendingTopicsResponse
{
    public int RecentPeriodStart { get; init; }
    public int RecentPeriodEnd { get; init; }
    public int PreviousPeriodStart { get; init; }
    public int PreviousPeriodEnd { get; init; }
    public List<TrendingTopicItemDto> Items { get; init; } = new();
}

public record ResearchTopicListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public int PapersCount { get; init; }
}
