using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ReferenceData.Queries;

public sealed record GetJournalsQuery(
    string? Search,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<JournalListItemDto>>;

public sealed record GetResearchTopicsQuery(
    string? Search,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<ResearchTopicListItemDto>>;
