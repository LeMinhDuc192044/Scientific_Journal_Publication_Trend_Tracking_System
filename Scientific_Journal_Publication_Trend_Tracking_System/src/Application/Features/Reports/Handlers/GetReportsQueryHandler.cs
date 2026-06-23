using MediatR;
using Microsoft.EntityFrameworkCore;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Helpers;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Handlers;

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, ReportListResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReportsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReportListResponse> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 50 ? 10 : request.PageSize;

        var query = _unitOfWork.DashboardReports.GetQueryable()
            .OrderByDescending(r => r.GeneratedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var reports = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ReportListResponse
        {
            Items = reports.Select(ReportDataBuilder.FromEntity).ToList(),
            TotalCount = totalCount
        };
    }
}

public class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, AnalyticalReportDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReportByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AnalyticalReportDto?> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await _unitOfWork.DashboardReports.GetByIdAsync(request.ReportId, cancellationToken);
        return report == null ? null : ReportDataBuilder.FromEntity(report);
    }
}
