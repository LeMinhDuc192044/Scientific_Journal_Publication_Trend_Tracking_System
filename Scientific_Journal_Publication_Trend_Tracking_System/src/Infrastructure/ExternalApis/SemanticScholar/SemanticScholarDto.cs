namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis.SemanticScholar;

/// <summary>
/// DTOs for Semantic Scholar API responses
/// </summary>
public record SemanticScholarPaperResponse(
    string PaperId,
    string Title,
    string? Abstract,
    int? Year,
    string[]? Keywords,
    SemanticScholarAuthor[]? Authors,
    string? Url,
    int? CitationCount,
    string? Venue);

public record SemanticScholarAuthor(
    string AuthorId,
    string Name,
    string? Url);

public record SemanticScholarSearchResponse(
    SemanticScholarPaperResponse[]? Data,
    int? Total);
