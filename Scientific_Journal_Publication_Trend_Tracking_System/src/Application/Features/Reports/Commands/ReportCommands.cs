using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Reports.Commands;

public record GenerateAnalyticalReportCommand(string? Title) : IRequest<AnalyticalReportDto>;
