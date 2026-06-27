
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis.SemanticScholar;
using Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.ExternalApis.ExternalPaperDto;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.ExternalApis;

public class ExternalApiSyncService : IExternalApiSyncService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiSyncService> _logger;
    private readonly IConfiguration _configuration;

    private const string SemanticScholarBaseUrl = "https://api.semanticscholar.org/graph/v1/paper/search";
    private const string OpenAlexBaseUrl = "https://api.openalex.org/works";
    private const string CrossrefBaseUrl = "https://api.crossref.org/works";
    private const string PoliteMail = "sit96leminhduc@example.com";


    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public ExternalApiSyncService(
        HttpClient httpClient,
        ILogger<ExternalApiSyncService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ExternalPaperDto?> GetByLinkAsync(
            string link, string apiSource, CancellationToken cancellationToken = default)
    {
        var identifier = ExtractIdentifier(link, apiSource);

        return apiSource.ToLower() switch
        {
            "semanticscholar" => await GetSemanticScholarByIdAsync(identifier, cancellationToken),
            "openalex" => await GetOpenAlexByIdAsync(identifier, cancellationToken),
            "crossref" => await GetCrossrefByDoiAsync(identifier, cancellationToken),
            _ => throw new ArgumentException($"Unknown source '{apiSource}'.")
        };
    }

    /// - A doi.org URL → the raw DOI (e.g. "10.1145/3442188.3445922")
    /// - A raw DOI already starting with "10." → returned as-is
    /// - Anything else (an OpenAlex ID, S2 paper ID, etc.) → returned as-is, trimmed
    /// </summary>
    private static string ExtractIdentifier(string link, string apiSource)
    {
        link = link.Trim();

        if (Uri.TryCreate(link, UriKind.Absolute, out var uri))
        {
            if (uri.Host.Contains("doi.org", StringComparison.OrdinalIgnoreCase))
                return uri.AbsolutePath.TrimStart('/');

            // OpenAlex URL form: https://openalex.org/W2741809807 → take the ID segment
            if (apiSource == "openalex" && uri.Host.Contains("openalex.org", StringComparison.OrdinalIgnoreCase))
                return uri.Segments.Last();
        }

        return link;
    }

    // ── Semantic Scholar ──────────────────────────────────────────────────────

    public async Task<List<ExternalPaperDto>> SearchSemanticScholarAsync(
            string query, int limit = 100, CancellationToken cancellationToken = default)
        {
            var url = $"{SemanticScholarBaseUrl}" +
                      $"?query={Uri.EscapeDataString(query)}" +
                      $"&limit={Math.Min(limit, 100)}" +
                      $"&fields=paperId,title,abstract,year,keywords,authors,url,citationCount,venue,externalIds";

            _logger.LogInformation("Calling Semantic Scholar API: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var raw = JsonSerializer.Deserialize<SemanticScholarSearchResponse>(content, JsonOpts);

            return raw?.Data?
                       .Select(MapSemanticScholar)
                       .ToList()
                   ?? [];
        }


    private async Task<ExternalPaperDto?> GetSemanticScholarByIdAsync(
    string identifier, CancellationToken ct)
    {
        // S2 supports prefixed identifiers: DOI:<doi>, ARXIV:<id>, or a raw S2 paperId
        var s2Id = identifier.StartsWith("10.", StringComparison.Ordinal)
            ? $"DOI:{identifier}"
            : identifier;

        var url = $"{SemanticScholarBaseUrl}/{Uri.EscapeDataString(s2Id)}" +
                  "?fields=paperId,title,abstract,year,keywords,authors,url,citationCount,venue";

        _logger.LogInformation("Calling Semantic Scholar single-paper lookup: {Url}", url);

        var response = await _httpClient.GetAsync(url, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        var paper = JsonSerializer.Deserialize<SemanticScholarPaperResponse>(content, JsonOpts);

        return paper is null ? null : MapSemanticScholar(paper);
    }

    private async Task<ExternalPaperDto?> GetOpenAlexByIdAsync(
        string identifier, CancellationToken ct)
    {
        // OpenAlex accepts "doi:<doi>" or a native OpenAlex ID (e.g. W2741809807)
        var oaId = identifier.StartsWith("10.", StringComparison.Ordinal)
            ? $"doi:{identifier}"
            : identifier;

        var url = $"{OpenAlexBaseUrl}/{Uri.EscapeDataString(oaId)}?mailto={PoliteMail}";

        _logger.LogInformation("Calling OpenAlex single-paper lookup: {Url}", url);

        var response = await _httpClient.GetAsync(url, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        var work = JsonNode.Parse(content);

        return work is null ? null : MapOpenAlex(work);
    }

    private async Task<ExternalPaperDto?> GetCrossrefByDoiAsync(
        string identifier, CancellationToken ct)
    {
        if (!identifier.StartsWith("10.", StringComparison.Ordinal))
            throw new ArgumentException(
                "Crossref lookup requires a DOI (e.g. 10.1145/xxxx). " +
                $"The link/identifier provided ('{identifier}') isn't a DOI.");

        var url = $"{CrossrefBaseUrl}/{Uri.EscapeDataString(identifier)}?mailto={PoliteMail}";

        _logger.LogInformation("Calling Crossref single-paper lookup: {Url}", url);

        var response = await _httpClient.GetAsync(url, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct);
        var json = JsonNode.Parse(content);
        var item = json?["message"];

        return item is null ? null : MapCrossref(item);
    }

    // Maps YOUR SemanticScholarPaperResponse → shared ExternalPaperDto
    private static ExternalPaperDto MapSemanticScholar(SemanticScholarPaperResponse p) => new(
            PaperId: p.PaperId,
            Title: p.Title,
            Abstract: p.Abstract,
            Keywords: p.Keywords,
            Year: p.Year,
            Url: p.Url,
            CitationCount: p.CitationCount,
            Doi: null,               // add ExternalIds?.DOI to your DTO if needed
            JournalName: p.Venue,
            Authors: p.Authors?
                             .Select(a => new ExternalAuthorDto(a.Name, a.AuthorId))
                             .ToList()
        );

        // ── OpenAlex ──────────────────────────────────────────────────────────────

        public async Task<List<ExternalPaperDto>> SearchOpenAlexAsync(
            string query, int limit = 100, CancellationToken cancellationToken = default)
        {
            var url = $"{OpenAlexBaseUrl}" +
                      $"?search={Uri.EscapeDataString(query)}" +
                      $"&per_page={Math.Min(limit, 200)}" +
                      $"&mailto=your-email@example.com";

            _logger.LogInformation("Calling OpenAlex API: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JsonNode.Parse(content);

            // OpenAlex returns: { "results": [ { "id", "title", "doi", ... } ] }
            var results = json?["results"]?.AsArray() ?? [];

            return results
                   .Where(r => r is not null)
                   .Select(r => MapOpenAlex(r!))
                   .ToList();
        }

        private static ExternalPaperDto MapOpenAlex(JsonNode work)
        {
            var id = work["id"]?.GetValue<string>() ?? Guid.NewGuid().ToString();
            var doi = work["doi"]?.GetValue<string>()
                                 ?.Replace("https://doi.org/", "");

            // Abstract: OpenAlex stores it as an inverted index {"word": [pos1, pos2]}
            string? abstractText = null;
            var invertedIndex = work["abstract_inverted_index"];
            if (invertedIndex is not null)
            {
                var positions = invertedIndex.AsObject()
                    .SelectMany(kv => kv.Value!.AsArray()
                        .Select(pos => (Position: pos!.GetValue<int>(), Word: kv.Key)))
                    .OrderBy(t => t.Position)
                    .Select(t => t.Word);
                abstractText = string.Join(" ", positions);
            }

            // Keywords: use concepts with score > 0.3
            var keywords = work["concepts"]?.AsArray()
                .Where(c => c?["score"]?.GetValue<double>() > 0.3)
                .Select(c => c?["display_name"]?.GetValue<string>())
                .Where(k => k is not null)
                .Cast<string>()
                .ToArray();

            // Authors
            var authors = work["authorships"]?.AsArray()
                .Select(a =>
                {
                    var authorNode = a?["author"];
                    var name = authorNode?["display_name"]?.GetValue<string>() ?? "Unknown";
                    var oaId = authorNode?["id"]?.GetValue<string>()?.Split('/').LastOrDefault();
                    return new ExternalAuthorDto(name, oaId);
                })
                .ToList();

            // Journal
            var journalName = work["primary_location"]?["source"]?["display_name"]?.GetValue<string>();

            return new ExternalPaperDto(
                PaperId: id.Split('/').LastOrDefault() ?? id,
                Title: work["title"]?.GetValue<string>() ?? string.Empty,
                Abstract: abstractText,
                Keywords: keywords is { Length: > 0 } ? keywords : null,
                Year: work["publication_year"]?.GetValue<int?>(),
                Url: doi is not null ? $"https://doi.org/{doi}" : null,
                CitationCount: work["cited_by_count"]?.GetValue<int?>(),
                Doi: doi,
                JournalName: journalName,
                Authors: authors
            );
        }

        // ── Crossref ──────────────────────────────────────────────────────────────

        public async Task<List<ExternalPaperDto>> SearchCrossrefAsync(
            string query, int limit = 100, CancellationToken cancellationToken = default)
        {
            var url = $"{CrossrefBaseUrl}" +
                      $"?query={Uri.EscapeDataString(query)}" +
                      $"&rows={Math.Min(limit, 100)}" +
                      $"&mailto=your-email@example.com" +
                      $"&select=DOI,title,abstract,published,subject,author,container-title,URL,is-referenced-by-count";

            _logger.LogInformation("Calling Crossref API: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JsonNode.Parse(content);

            // Crossref returns: { "message": { "items": [ ... ] } }
            var items = json?["message"]?["items"]?.AsArray() ?? [];

            return items
                   .Where(i => i is not null)
                   .Select(i => MapCrossref(i!))
                   .ToList();
        }

        private static ExternalPaperDto MapCrossref(JsonNode item)
        {
            var doi = item["DOI"]?.GetValue<string>();
            var title = item["title"]?.AsArray().FirstOrDefault()?.GetValue<string>() ?? string.Empty;

            // Year: stored as { "date-parts": [[2023, 5, 1]] }
            int? year = null;
            var dateParts = item["published"]?["date-parts"]?.AsArray()
                                .FirstOrDefault()?.AsArray();
            if (dateParts is { Count: > 0 })
                year = dateParts[0]?.GetValue<int>();

            // Strip JATS XML tags from abstract (e.g. <jats:p>)
            var rawAbstract = item["abstract"]?.GetValue<string>();
            var abstractText = rawAbstract is null
                ? null
                : System.Text.RegularExpressions.Regex.Replace(rawAbstract, "<[^>]+>", " ").Trim();

            // Authors
            var authors = item["author"]?.AsArray()
                .Select(a =>
                {
                    var given = a?["given"]?.GetValue<string>();
                    var family = a?["family"]?.GetValue<string>();
                    var name = string.Join(" ",
                        new[] { given, family }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    var orcid = a?["ORCID"]?.GetValue<string>()
                                  ?.Replace("http://orcid.org/", "")
                                  ?.Replace("https://orcid.org/", "");
                    return new ExternalAuthorDto(
                        string.IsNullOrEmpty(name) ? "Unknown" : name, orcid);
                })
                .ToList();

            // Journal + Keywords (subjects)
            var journal = item["container-title"]?.AsArray().FirstOrDefault()?.GetValue<string>();
            var keywords = item["subject"]?.AsArray()
                .Select(s => s?.GetValue<string>())
                .Where(s => s is not null)
                .Cast<string>()
                .ToArray();

            return new ExternalPaperDto(
                PaperId: doi ?? Guid.NewGuid().ToString(),
                Title: title,
                Abstract: abstractText,
                Keywords: keywords is { Length: > 0 } ? keywords : null,
                Year: year,
                Url: item["URL"]?.GetValue<string>() ?? (doi is not null ? $"https://doi.org/{doi}" : null),
                CitationCount: item["is-referenced-by-count"]?.GetValue<int?>(),
                Doi: doi,
                JournalName: journal,
                Authors: authors
            );
        }
    }