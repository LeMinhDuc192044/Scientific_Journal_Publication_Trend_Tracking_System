using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;

/// <summary>
/// Query to get research paper details by ID
/// </summary>
public record GetResearchPaperDetailQuery(Guid PaperId) : IRequest<ResearchPaperDto?>;
