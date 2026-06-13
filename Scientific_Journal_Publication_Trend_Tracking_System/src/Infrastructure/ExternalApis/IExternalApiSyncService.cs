using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis.SemanticScholar;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.ExternalApis.ExternalPaperDto;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis;

/// <summary>
/// Interface for synchronizing research papers from external APIs
/// </summary>
public interface IExternalApiSyncService
{
    Task<List<ExternalPaperDto>> SearchSemanticScholarAsync(string query, int limit = 100, CancellationToken cancellationToken = default);
    Task<List<ExternalPaperDto>> SearchOpenAlexAsync(string query, int limit = 100, CancellationToken cancellationToken = default);
    Task<List<ExternalPaperDto>> SearchCrossrefAsync(string query, int limit = 100, CancellationToken cancellationToken = default);
}
