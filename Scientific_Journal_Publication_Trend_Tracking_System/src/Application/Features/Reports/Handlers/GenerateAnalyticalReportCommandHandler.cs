using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Commands;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Helpers;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Handlers;

public class GenerateAnalyticalReportCommandHandler : IRequestHandler<GenerateAnalyticalReportCommand, AnalyticalReportDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GenerateAnalyticalReportCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AnalyticalReportDto> Handle(GenerateAnalyticalReportCommand request, CancellationToken cancellationToken)
    {
        var summary = await ReportDataBuilder.BuildAsync(_unitOfWork, cancellationToken);
        var entity = ReportDataBuilder.ToEntity(summary, request.Title);

        await _unitOfWork.DashboardReports.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = ReportDataBuilder.FromEntity(entity);
        return saved with { Title = entity.Title };
    }
}

public class GetAnalyticalReportSummaryQueryHandler : IRequestHandler<GetAnalyticalReportSummaryQuery, AnalyticalReportDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAnalyticalReportSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AnalyticalReportDto> Handle(GetAnalyticalReportSummaryQuery request, CancellationToken cancellationToken)
    {
        return await ReportDataBuilder.BuildAsync(_unitOfWork, cancellationToken);
    }
}
