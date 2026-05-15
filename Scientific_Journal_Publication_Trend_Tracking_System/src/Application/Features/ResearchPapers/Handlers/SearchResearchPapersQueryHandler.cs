using AutoMapper;
using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.Authentication.Handlers;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;
using Scientific_Journal_Publication_Trend_Tracking_System.Shared.Results;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Handlers;

/// <summary>
/// Handler for searching research papers
/// </summary>
public class SearchResearchPapersQueryHandler : IRequestHandler<SearchResearchPapersQuery, PagedResult<ResearchPaperDto>>
{
    private readonly IResearchPaperRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<LoginCommandHandler> _logger;

    public SearchResearchPapersQueryHandler(
        IResearchPaperRepository repository,
        IMapper mapper,
        ILogger<LoginCommandHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ResearchPaperDto>> Handle(
        SearchResearchPapersQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Searching research papers - Keyword: {Keyword}, Author: {Author}, Domain: {Domain}, Page: {Page}",
            request.Keyword, request.Author, request.Domain, request.PageNumber);

        // Get total count
        var totalCount = await _repository.GetSearchCountAsync(
            request.Keyword,
            request.Author,
            request.Journal,
            request.FromYear,
            request.ToYear,
            request.Domain,
            cancellationToken);

        // Get paginated results
        var papers = await _repository.SearchAsync(
            request.Keyword,
            request.Author,
            request.Journal,
            request.FromYear,
            request.ToYear,
            request.Domain,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var mappedPapers = _mapper.Map<List<ResearchPaperDto>>(papers);

        _logger.LogInformation("Search completed - Found {TotalCount} papers", totalCount);

        return new PagedResult<ResearchPaperDto>
        {
            Items = mappedPapers,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
