using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Queries;

public record GetKeywordTrendQuery(
    string Keyword,
    int? FromYear = null,
    int? ToYear = null) : IRequest<KeywordTrendResponse>;

public record GetTopicTrendQuery(
    Guid TopicId,
    int? FromYear = null,
    int? ToYear = null) : IRequest<TopicTrendResponse?>;

public record GetTrendingTopicsQuery(
    int TopCount = 10,
    int RecentYears = 2) : IRequest<TrendingTopicsResponse>;

public record GetResearchTopicsQuery() : IRequest<List<ResearchTopicListItemDto>>;
