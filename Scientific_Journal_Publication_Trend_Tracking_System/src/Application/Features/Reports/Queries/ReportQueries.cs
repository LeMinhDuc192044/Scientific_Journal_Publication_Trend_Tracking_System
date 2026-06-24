using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Queries;

public record GetAnalyticalReportSummaryQuery : IRequest<AnalyticalReportDto>;

public record GetReportsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<ReportListResponse>;

public record GetReportByIdQuery(Guid ReportId) : IRequest<AnalyticalReportDto?>;
