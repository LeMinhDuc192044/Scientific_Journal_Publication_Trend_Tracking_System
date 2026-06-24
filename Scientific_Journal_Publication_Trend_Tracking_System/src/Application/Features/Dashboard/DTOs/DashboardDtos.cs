namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.DTOs;

public record DashboardSummaryResponse
{
    public int TotalPapers { get; init; }
    public int PapersThisYear { get; init; }
    public int TotalJournals { get; init; }
    public int TotalAuthors { get; init; }
    public double AverageCitations { get; init; }
    public string TopDomain { get; init; } = string.Empty;
    public int TotalResearchTopics { get; init; }
}

public record ChartDataPointDto
{
    public string Label { get; init; } = string.Empty;
    public double Value { get; init; }
}

public record ChartResponse
{
    public string ChartType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public List<string> Labels { get; init; } = new();
    public List<double> Values { get; init; } = new();
    public List<ChartDataPointDto> DataPoints { get; init; } = new();
}
