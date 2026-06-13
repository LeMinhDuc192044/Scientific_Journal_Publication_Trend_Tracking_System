using AutoMapper;
using MediatR;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.DTOs;
using Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Queries;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence.Repositories;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Handlers;

/// <summary>
/// Handler for getting popular research papers
/// </summary>
public class GetPopularResearchPapersQueryHandler : IRequestHandler<GetPopularResearchPapersQuery, List<ResearchPaperDto>>
{
    private readonly IResearchPaperRepository _repository;
    private readonly IMapper _mapper;

    public GetPopularResearchPapersQueryHandler(IResearchPaperRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<ResearchPaperDto>> Handle(GetPopularResearchPapersQuery request, CancellationToken cancellationToken)
    {
        var papers = await _repository.GetPopularPapersAsync(request.TopCount, cancellationToken);
        return papers.Select(p => _mapper.Map<ResearchPaperDto>(p)).ToList();
    }
}
