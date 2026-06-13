using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;

/// <summary>
/// Query to get popular research papers (by citation count)
/// </summary>
public record GetPopularResearchPapersQuery(int TopCount = 10) : IRequest<List<ResearchPaperDto>>;
