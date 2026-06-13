using AutoMapper;
using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Handlers;

/// <summary>
/// Handler for searching research papers
/// </summary>
public class SearchResearchPapersQueryHandler : IRequestHandler<SearchResearchPapersQuery, SearchPapersResponse>
{
    private readonly IResearchPaperRepository _repository;
    private readonly IMapper _mapper;

    public SearchResearchPapersQueryHandler(IResearchPaperRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SearchPapersResponse> Handle(SearchResearchPapersQuery request, CancellationToken cancellationToken)
    {
        // Determine domain from environment configuration or default to all
        ResearchDomain? domain = null;

        // Get total count for pagination
        var totalCount = await _repository.GetSearchCountAsync(
            request.Keyword,
            request.Author,
            request.Journal,
            request.FromYear,
            request.ToYear,
            domain,
            cancellationToken);

        // Get paginated results
        var papers = await _repository.SearchAsync(
            request.Keyword,
            request.Author,
            request.Journal,
            request.FromYear,
            request.ToYear,
            domain,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var paperDtos = papers.Select(p => _mapper.Map<ResearchPaperDto>(p)).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return new SearchPapersResponse(
            paperDtos,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);
    }
}
