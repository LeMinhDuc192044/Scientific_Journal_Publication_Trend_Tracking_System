namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis.OpenAlex;

/// <summary>
/// DTOs for OpenAlex API responses
/// </summary>
public record OpenAlexWork(
    string Id,
    string Title,
    string? Abstract,
    int? PublicationYear,
    string[]? Keywords,
    OpenAlexAuthor[]? AuthorshipNodes,
    string? DoiUrl,
    int? CitedByCount,
    OpenAlexVenue? PrimaryLocation);

public record OpenAlexAuthor(
    OpenAlexAuthorInfo? Author);

public record OpenAlexAuthorInfo(
    string Id,
    string Name);

public record OpenAlexVenue(
    OpenAlexHost? Host);

public record OpenAlexHost(
    string DisplayName);

public record OpenAlexSearchResponse(
    OpenAlexWork[]? Results,
    int? Meta);
