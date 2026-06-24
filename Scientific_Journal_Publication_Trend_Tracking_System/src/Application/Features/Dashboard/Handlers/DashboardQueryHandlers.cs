using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.Handlers;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    private readonly ITrendAnalyticsRepository _repository;

    public GetDashboardSummaryQueryHandler(ITrendAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<DashboardSummaryResponse> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken) =>
        _repository.GetDashboardSummaryAsync(cancellationToken);
}

public class GetPublicationsByYearChartQueryHandler : IRequestHandler<GetPublicationsByYearChartQuery, ChartResponse>
{
    private readonly ITrendAnalyticsRepository _repository;

    public GetPublicationsByYearChartQueryHandler(ITrendAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<ChartResponse> Handle(GetPublicationsByYearChartQuery request, CancellationToken cancellationToken) =>
        _repository.GetPublicationsByYearChartAsync(request.FromYear, request.ToYear, cancellationToken);
}

public class GetTopKeywordsChartQueryHandler : IRequestHandler<GetTopKeywordsChartQuery, ChartResponse>
{
    private readonly ITrendAnalyticsRepository _repository;

    public GetTopKeywordsChartQueryHandler(ITrendAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<ChartResponse> Handle(GetTopKeywordsChartQuery request, CancellationToken cancellationToken) =>
        _repository.GetTopKeywordsChartAsync(request.TopCount, cancellationToken);
}

public class GetPublicationsByDomainChartQueryHandler : IRequestHandler<GetPublicationsByDomainChartQuery, ChartResponse>
{
    private readonly ITrendAnalyticsRepository _repository;

    public GetPublicationsByDomainChartQueryHandler(ITrendAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<ChartResponse> Handle(GetPublicationsByDomainChartQuery request, CancellationToken cancellationToken) =>
        _repository.GetPublicationsByDomainChartAsync(cancellationToken);
}

public class GetTopJournalsChartQueryHandler : IRequestHandler<GetTopJournalsChartQuery, ChartResponse>
{
    private readonly ITrendAnalyticsRepository _repository;

    public GetTopJournalsChartQueryHandler(ITrendAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<ChartResponse> Handle(GetTopJournalsChartQuery request, CancellationToken cancellationToken) =>
        _repository.GetTopJournalsChartAsync(request.TopCount, cancellationToken);
}
