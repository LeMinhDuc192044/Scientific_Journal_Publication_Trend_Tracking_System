using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Trends.Handlers;

public class GetKeywordTrendQueryHandler : IRequestHandler<GetKeywordTrendQuery, KeywordTrendResponse>
{
    private readonly ITrendAnalyticsRepository _repository;

    public GetKeywordTrendQueryHandler(ITrendAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<KeywordTrendResponse> Handle(GetKeywordTrendQuery request, CancellationToken cancellationToken) =>
        _repository.GetKeywordTrendAsync(request.Keyword, request.FromYear, request.ToYear, cancellationToken);
}

public class GetTopicTrendQueryHandler : IRequestHandler<GetTopicTrendQuery, TopicTrendResponse?>
{
    private readonly ITrendAnalyticsRepository _repository;

    public GetTopicTrendQueryHandler(ITrendAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<TopicTrendResponse?> Handle(GetTopicTrendQuery request, CancellationToken cancellationToken) =>
        _repository.GetTopicTrendAsync(request.TopicId, request.FromYear, request.ToYear, cancellationToken);
}

public class GetTrendingTopicsQueryHandler : IRequestHandler<GetTrendingTopicsQuery, TrendingTopicsResponse>
{
    private readonly ITrendAnalyticsRepository _repository;

    public GetTrendingTopicsQueryHandler(ITrendAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<TrendingTopicsResponse> Handle(GetTrendingTopicsQuery request, CancellationToken cancellationToken) =>
        _repository.GetTrendingTopicsAsync(request.TopCount, request.RecentYears, cancellationToken);
}

public class GetResearchTopicsQueryHandler : IRequestHandler<GetResearchTopicsQuery, List<ResearchTopicListItemDto>>
{
    private readonly AppDbContext _context;

    public GetResearchTopicsQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResearchTopicListItemDto>> Handle(
        GetResearchTopicsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.ResearchTopics
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new ResearchTopicListItemDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Domain = t.Domain.ToString(),
                PapersCount = t.PapersCount
            })
            .ToListAsync(cancellationToken);
    }
}
