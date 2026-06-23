using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Dashboard.Queries;

public record GetDashboardSummaryQuery() : IRequest<DashboardSummaryResponse>;

public record GetPublicationsByYearChartQuery(
    int? FromYear = null,
    int? ToYear = null) : IRequest<ChartResponse>;

public record GetTopKeywordsChartQuery(int TopCount = 10) : IRequest<ChartResponse>;

public record GetPublicationsByDomainChartQuery() : IRequest<ChartResponse>;

public record GetTopJournalsChartQuery(int TopCount = 10) : IRequest<ChartResponse>;
