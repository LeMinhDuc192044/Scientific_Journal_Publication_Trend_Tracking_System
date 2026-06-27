using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.DTO;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Application.Features.ResearchTopics.Commands;


public record SearchResearchTopicsByNameQuery(
    string Name,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<ResearchTopicDto>>;