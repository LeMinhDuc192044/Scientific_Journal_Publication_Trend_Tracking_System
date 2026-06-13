using MediatR;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Application.Features.ResearchPapers.Commands;

/// <summary>
/// Command to synchronize research papers from external APIs
/// </summary>
public record SyncResearchPapersCommand(
    string Query,
    string ApiSource,
    int Limit = 100) : IRequest<SyncPapersResponse>;

/// <summary>
/// Response for sync research papers command
/// </summary>
public record SyncPapersResponse(
    int TotalSynced,
    int TotalCreated,
    int TotalUpdated,
    List<string> Errors);
