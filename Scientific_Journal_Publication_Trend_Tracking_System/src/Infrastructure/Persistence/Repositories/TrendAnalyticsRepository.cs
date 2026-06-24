using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

public class TrendAnalyticsRepository : ITrendAnalyticsRepository
{
    private readonly AppDbContext _context;

    public TrendAnalyticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<KeywordTrendResponse> GetKeywordTrendAsync(
        string keyword,
        int? fromYear,
        int? toYear,
        CancellationToken cancellationToken = default)
    {
        var normalizedKeyword = keyword.Trim().ToLowerInvariant();
        var (startYear, endYear) = ResolveYearRange(fromYear, toYear);

        var papers = await GetPapersMatchingKeywordAsync(normalizedKeyword, startYear, endYear, cancellationToken);
        var dataPoints = BuildTrendDataPoints(papers);

        return new KeywordTrendResponse
        {
            Keyword = keyword.Trim(),
            FromYear = startYear,
            ToYear = endYear,
            DataPoints = dataPoints,
            TotalPublications = papers.Count,
            OverallGrowthRate = CalculateOverallGrowthRate(dataPoints)
        };
    }

    public async Task<TopicTrendResponse?> GetTopicTrendAsync(
        Guid topicId,
        int? fromYear,
        int? toYear,
        CancellationToken cancellationToken = default)
    {
        var topic = await _context.ResearchTopics
            .AsNoTracking()
            .Include(t => t.Keywords)
            .FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken);

        if (topic is null)
            return null;

        var (startYear, endYear) = ResolveYearRange(fromYear, toYear);
        var matchTerms = BuildTopicMatchTerms(topic);

        var papers = await _context.ResearchPapers
            .AsNoTracking()
            .Where(p => p.PublicationYear >= startYear && p.PublicationYear <= endYear)
            .Select(p => new
            {
                p.PublicationYear,
                p.CitationCount,
                p.Title,
                p.Abstract,
                p.Keywords,
                TopicIds = p.ResearchTopics.Select(t => t.Id).ToList()
            })
            .ToListAsync(cancellationToken);

        var matched = papers
            .Where(p =>
                p.TopicIds.Contains(topicId) ||
                matchTerms.Any(term =>
                    p.Title.ToLower().Contains(term) ||
                    (p.Abstract != null && p.Abstract.ToLower().Contains(term)) ||
                    p.Keywords.Any(k => k.ToLower().Contains(term))))
            .Select(p => new PaperTrendRecord(p.PublicationYear, p.CitationCount))
            .ToList();

        var dataPoints = BuildTrendDataPoints(matched);

        return new TopicTrendResponse
        {
            TopicId = topic.Id,
            TopicName = topic.Name,
            FromYear = startYear,
            ToYear = endYear,
            DataPoints = dataPoints,
            TotalPublications = matched.Count,
            OverallGrowthRate = CalculateOverallGrowthRate(dataPoints)
        };
    }

    public async Task<TrendingTopicsResponse> GetTrendingTopicsAsync(
        int topCount,
        int recentYears,
        CancellationToken cancellationToken = default)
    {
        recentYears = Math.Clamp(recentYears, 1, 5);

        var maxYear = await _context.ResearchPapers
            .MaxAsync(p => (int?)p.PublicationYear, cancellationToken)
            ?? DateTime.UtcNow.Year;

        var recentStart = maxYear - recentYears + 1;
        var recentEnd = maxYear;
        var previousStart = recentStart - recentYears;
        var previousEnd = recentStart - 1;

        var minYear = Math.Max(previousStart, 1900);

        var papers = await _context.ResearchPapers
            .AsNoTracking()
            .Where(p => p.PublicationYear >= minYear)
            .Select(p => new { p.PublicationYear, p.Keywords })
            .ToListAsync(cancellationToken);

        var keywordStats = papers
            .SelectMany(p => p.Keywords
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => new { Keyword = NormalizeKeyword(k), p.PublicationYear }))
            .GroupBy(x => x.Keyword)
            .Select(g => new
            {
                Keyword = g.Key,
                Total = g.Count(),
                Recent = g.Count(x => x.PublicationYear >= recentStart && x.PublicationYear <= recentEnd),
                Previous = g.Count(x => x.PublicationYear >= previousStart && x.PublicationYear <= previousEnd)
            })
            .Where(x => x.Recent >= 1)
            .Select(x => new
            {
                x.Keyword,
                x.Total,
                x.Recent,
                x.Previous,
                GrowthRate = CalculateGrowthRate(x.Recent, x.Previous)
            })
            .OrderByDescending(x => x.GrowthRate)
            .ThenByDescending(x => x.Recent)
            .Take(topCount)
            .ToList();

        var items = keywordStats
            .Select((x, index) => new TrendingTopicItemDto
            {
                Rank = index + 1,
                Name = x.Keyword,
                Type = "Keyword",
                RecentCount = x.Recent,
                PreviousCount = x.Previous,
                GrowthRate = Math.Round(x.GrowthRate, 4),
                TotalPapers = x.Total
            })
            .ToList();

        var seededTopics = await _context.ResearchTopics
            .AsNoTracking()
            .Include(t => t.Keywords)
            .ToListAsync(cancellationToken);

        foreach (var topic in seededTopics)
        {
            var matchTerms = BuildTopicMatchTerms(topic);
            var topicPapers = papers.Where(p =>
                matchTerms.Any(term =>
                    p.Keywords.Any(k => k.ToLower().Contains(term))));

            var recent = topicPapers.Count(p => p.PublicationYear >= recentStart && p.PublicationYear <= recentEnd);
            var previous = topicPapers.Count(p => p.PublicationYear >= previousStart && p.PublicationYear <= previousEnd);

            if (recent < 1)
                continue;

            items.Add(new TrendingTopicItemDto
            {
                Rank = 0,
                Name = topic.Name,
                Type = "ResearchTopic",
                RecentCount = recent,
                PreviousCount = previous,
                GrowthRate = Math.Round(CalculateGrowthRate(recent, previous), 4),
                TotalPapers = topicPapers.Count()
            });
        }

        items = items
            .OrderByDescending(x => x.GrowthRate)
            .ThenByDescending(x => x.RecentCount)
            .Take(topCount)
            .Select((x, index) => x with { Rank = index + 1 })
            .ToList();

        return new TrendingTopicsResponse
        {
            RecentPeriodStart = recentStart,
            RecentPeriodEnd = recentEnd,
            PreviousPeriodStart = previousStart,
            PreviousPeriodEnd = previousEnd,
            Items = items
        };
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var currentYear = DateTime.UtcNow.Year;

        var totalPapers = await _context.ResearchPapers.CountAsync(cancellationToken);
        var papersThisYear = await _context.ResearchPapers
            .CountAsync(p => p.PublicationYear == currentYear, cancellationToken);
        var totalJournals = await _context.Journals.CountAsync(cancellationToken);
        var totalAuthors = await _context.Authors.CountAsync(cancellationToken);
        var totalTopics = await _context.ResearchTopics.CountAsync(cancellationToken);

        var avgCitations = totalPapers == 0
            ? 0
            : await _context.ResearchPapers.AverageAsync(p => (double)p.CitationCount, cancellationToken);

        var topDomain = await _context.ResearchPapers
            .AsNoTracking()
            .GroupBy(p => p.Domain)
            .Select(g => new { Domain = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync(cancellationToken);

        return new DashboardSummaryResponse
        {
            TotalPapers = totalPapers,
            PapersThisYear = papersThisYear,
            TotalJournals = totalJournals,
            TotalAuthors = totalAuthors,
            AverageCitations = Math.Round(avgCitations, 2),
            TopDomain = topDomain?.Domain.ToString() ?? ResearchDomain.ComputerScience.ToString(),
            TotalResearchTopics = totalTopics
        };
    }

    public async Task<ChartResponse> GetPublicationsByYearChartAsync(
        int? fromYear,
        int? toYear,
        CancellationToken cancellationToken = default)
    {
        var (startYear, endYear) = ResolveYearRange(fromYear, toYear);

        var data = await _context.ResearchPapers
            .AsNoTracking()
            .Where(p => p.PublicationYear >= startYear && p.PublicationYear <= endYear)
            .GroupBy(p => p.PublicationYear)
            .Select(g => new { Year = g.Key, Count = g.Count() })
            .OrderBy(x => x.Year)
            .ToListAsync(cancellationToken);

        return new ChartResponse
        {
            ChartType = "line",
            Title = "Publications by Year",
            Labels = data.Select(x => x.Year.ToString()).ToList(),
            Values = data.Select(x => (double)x.Count).ToList(),
            DataPoints = data.Select(x => new ChartDataPointDto
            {
                Label = x.Year.ToString(),
                Value = x.Count
            }).ToList()
        };
    }

    public async Task<ChartResponse> GetTopKeywordsChartAsync(
        int topCount,
        CancellationToken cancellationToken = default)
    {
        var papers = await _context.ResearchPapers
            .AsNoTracking()
            .Select(p => p.Keywords)
            .ToListAsync(cancellationToken);

        var keywordCounts = papers
            .SelectMany(kws => kws.Where(k => !string.IsNullOrWhiteSpace(k)))
            .Select(NormalizeKeyword)
            .GroupBy(k => k)
            .Select(g => new { Keyword = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(topCount)
            .ToList();

        return new ChartResponse
        {
            ChartType = "bar",
            Title = "Top Keywords",
            Labels = keywordCounts.Select(x => x.Keyword).ToList(),
            Values = keywordCounts.Select(x => (double)x.Count).ToList(),
            DataPoints = keywordCounts.Select(x => new ChartDataPointDto
            {
                Label = x.Keyword,
                Value = x.Count
            }).ToList()
        };
    }

    public async Task<ChartResponse> GetPublicationsByDomainChartAsync(
        CancellationToken cancellationToken = default)
    {
        var data = await _context.ResearchPapers
            .AsNoTracking()
            .GroupBy(p => p.Domain)
            .Select(g => new { Domain = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return new ChartResponse
        {
            ChartType = "pie",
            Title = "Publications by Research Domain",
            Labels = data.Select(x => x.Domain).ToList(),
            Values = data.Select(x => (double)x.Count).ToList(),
            DataPoints = data.Select(x => new ChartDataPointDto
            {
                Label = x.Domain,
                Value = x.Count
            }).ToList()
        };
    }

    public async Task<ChartResponse> GetTopJournalsChartAsync(
        int topCount,
        CancellationToken cancellationToken = default)
    {
        var data = await _context.ResearchPapers
            .AsNoTracking()
            .Where(p => p.JournalId != null)
            .GroupBy(p => p.Journal!.Title)
            .Select(g => new { Journal = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(topCount)
            .ToListAsync(cancellationToken);

        return new ChartResponse
        {
            ChartType = "horizontalBar",
            Title = "Top Journals by Publication Count",
            Labels = data.Select(x => x.Journal).ToList(),
            Values = data.Select(x => (double)x.Count).ToList(),
            DataPoints = data.Select(x => new ChartDataPointDto
            {
                Label = x.Journal,
                Value = x.Count
            }).ToList()
        };
    }

    private async Task<List<PaperTrendRecord>> GetPapersMatchingKeywordAsync(
        string normalizedKeyword,
        int startYear,
        int endYear,
        CancellationToken cancellationToken)
    {
        var papers = await _context.ResearchPapers
            .AsNoTracking()
            .Where(p => p.PublicationYear >= startYear && p.PublicationYear <= endYear)
            .Select(p => new
            {
                p.PublicationYear,
                p.CitationCount,
                p.Title,
                p.Abstract,
                p.Keywords
            })
            .ToListAsync(cancellationToken);

        return papers
            .Where(p =>
                p.Keywords.Any(k => k.ToLower().Contains(normalizedKeyword)) ||
                p.Title.ToLower().Contains(normalizedKeyword) ||
                (p.Abstract != null && p.Abstract.ToLower().Contains(normalizedKeyword)))
            .Select(p => new PaperTrendRecord(p.PublicationYear, p.CitationCount))
            .ToList();
    }

    private static List<TrendDataPointDto> BuildTrendDataPoints(IReadOnlyList<PaperTrendRecord> papers)
    {
        var grouped = papers
            .GroupBy(p => p.Year)
            .OrderBy(g => g.Key)
            .Select(g => new TrendDataPointDto
            {
                Year = g.Key,
                PublicationCount = g.Count(),
                AverageCitations = Math.Round(g.Average(p => p.CitationCount), 2),
                GrowthRate = 0
            })
            .ToList();

        for (var i = 0; i < grouped.Count; i++)
        {
            if (i == 0)
                continue;

            var previous = grouped[i - 1].PublicationCount;
            grouped[i] = grouped[i] with
            {
                GrowthRate = Math.Round(CalculateGrowthRate(grouped[i].PublicationCount, previous), 4)
            };
        }

        return grouped;
    }

    private static double CalculateOverallGrowthRate(IReadOnlyList<TrendDataPointDto> dataPoints)
    {
        if (dataPoints.Count < 2)
            return 0;

        var first = dataPoints.First().PublicationCount;
        var last = dataPoints.Last().PublicationCount;
        return Math.Round(CalculateGrowthRate(last, first), 4);
    }

    private static double CalculateGrowthRate(int current, int previous)
    {
        if (previous == 0)
            return current > 0 ? 1.0 : 0;

        return (double)(current - previous) / previous;
    }

    private static (int StartYear, int EndYear) ResolveYearRange(int? fromYear, int? toYear)
    {
        var currentYear = DateTime.UtcNow.Year;
        var start = fromYear ?? currentYear - 5;
        var end = toYear ?? currentYear;

        if (start > end)
            (start, end) = (end, start);

        return (start, end);
    }

    private static string NormalizeKeyword(string keyword) =>
        keyword.Trim().ToLowerInvariant();

    private static List<string> BuildTopicMatchTerms(Domain.Entities.ResearchTopic topic)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            topic.Name.Trim().ToLowerInvariant()
        };

        foreach (var keyword in topic.Keywords)
            terms.Add(keyword.Name.Trim().ToLowerInvariant());

        return terms.ToList();
    }

    private sealed record PaperTrendRecord(int Year, int CitationCount);
}
