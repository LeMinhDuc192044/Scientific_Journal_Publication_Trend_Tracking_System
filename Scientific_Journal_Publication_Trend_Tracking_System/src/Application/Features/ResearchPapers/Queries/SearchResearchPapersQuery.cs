using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;

/// <summary>
/// Query to search research papers with filters and pagination
/// </summary>
public record SearchResearchPapersQuery(
    string? Keyword,
    string? Author,
    string? Journal,
    int? FromYear,
    int? ToYear,
    int PageNumber,
    int PageSize,
    string SortBy,
    bool SortDescending) : IRequest<SearchPapersResponse>;

/// <summary>
/// Response for search research papers query
/// </summary>
public record SearchPapersResponse(
    List<ResearchPaperDto> Papers,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);
