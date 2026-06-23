using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

public interface ITrendAnalyticsRepository
{
    Task<KeywordTrendResponse> GetKeywordTrendAsync(
        string keyword,
        int? fromYear,
        int? toYear,
        CancellationToken cancellationToken = default);

    Task<TopicTrendResponse?> GetTopicTrendAsync(
        Guid topicId,
        int? fromYear,
        int? toYear,
        CancellationToken cancellationToken = default);

    Task<TrendingTopicsResponse> GetTrendingTopicsAsync(
        int topCount,
        int recentYears,
        CancellationToken cancellationToken = default);

    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<ChartResponse> GetPublicationsByYearChartAsync(
        int? fromYear,
        int? toYear,
        CancellationToken cancellationToken = default);

    Task<ChartResponse> GetTopKeywordsChartAsync(
        int topCount,
        CancellationToken cancellationToken = default);

    Task<ChartResponse> GetPublicationsByDomainChartAsync(
        CancellationToken cancellationToken = default);

    Task<ChartResponse> GetTopJournalsChartAsync(
        int topCount,
        CancellationToken cancellationToken = default);
}
