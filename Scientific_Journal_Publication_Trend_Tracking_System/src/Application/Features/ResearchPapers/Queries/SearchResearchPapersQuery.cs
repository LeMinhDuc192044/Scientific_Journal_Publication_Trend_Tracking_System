using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;

/// <summary>
/// Query to search research papers with various filters
/// </summary>
public record SearchResearchPapersQuery(
    string? Keyword,
    string? Author,
    string? Journal,
    int? FromYear,
    int? ToYear,
    ResearchDomain? Domain,
    int PageNumber,
    int PageSize,
    string SortBy,
    bool SortDescending) : IRequest<PagedResult<ResearchPaperDto>>;
