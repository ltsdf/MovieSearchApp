using MovieSearchApp.App.External.Omdb;
using MovieSearchApp.App.Models;
using MovieSearchApp.App.Options;

namespace MovieSearchApp.App.Services;

public interface IOmdbClient
{
    /// <summary>
    /// Searches movies by title. OMDb limits page size to10.
    /// Page is1-based, matching OMDb API expectations.
    /// </summary>
    Task<PagedResult<MovieSearchItem>> SearchAsync(string query, int page = 1, CancellationToken ct = default);
    Task<MovieDetailsDto?> GetDetailsAsync(string imdbId, CancellationToken ct = default);
}

public sealed class OmdbClient(HttpClient http, OmdbOptions options) : IOmdbClient
{
    private readonly HttpClient _http = http;
    private readonly OmdbOptions _options = options;
    private const int OmdbPageSize = 10;

    public async Task<PagedResult<MovieSearchItem>> SearchAsync(string query, int page = 1, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new PagedResult<MovieSearchItem> { Items = Array.Empty<MovieSearchItem>(), TotalCount = 0, Page = page, PageSize = OmdbPageSize };

        var url = $"{_options.BaseUrl.TrimEnd('/')}/?apikey={Uri.EscapeDataString(_options.ApiKey)}&s={Uri.EscapeDataString(query)}&type=movie&page={page}";
        var resp = await _http.GetFromJsonAsync<OmdbSearchResponse>(url, ct).ConfigureAwait(false);
        if (resp is null || !string.Equals(resp.Response, "True", StringComparison.OrdinalIgnoreCase) || resp.Search is null)
            return new PagedResult<MovieSearchItem> { Items = Array.Empty<MovieSearchItem>(), TotalCount = 0, Page = page, PageSize = OmdbPageSize };

        var items = resp.Search
        .Where(i => !string.IsNullOrWhiteSpace(i.ImdbID))
        .Select(i => new MovieSearchItem(
        i.Title ?? "(unknown)",
        i.Year ?? string.Empty,
        i.ImdbID!,
        i.Type ?? string.Empty,
        NormalizePoster(i.Poster)))
        .ToList();

        var total = int.TryParse(resp.TotalResults, out var t) ? t : items.Count;
        return new PagedResult<MovieSearchItem> { Items = items, TotalCount = total, Page = page, PageSize = OmdbPageSize };
    }

    public async Task<MovieDetailsDto?> GetDetailsAsync(string imdbId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imdbId)) return null;
        var url = $"{_options.BaseUrl.TrimEnd('/')}/?apikey={Uri.EscapeDataString(_options.ApiKey)}&i={Uri.EscapeDataString(imdbId)}&plot=full";
        var resp = await _http.GetFromJsonAsync<OmdbDetailsResponse>(url, ct).ConfigureAwait(false);
        if (resp is null || !string.Equals(resp.Response, "True", StringComparison.OrdinalIgnoreCase))
            return null;

        return new MovieDetailsDto(
        resp.Title ?? string.Empty,
        resp.Year ?? string.Empty,
        resp.Rated ?? string.Empty,
        resp.Released ?? string.Empty,
        resp.Runtime ?? string.Empty,
        resp.Genre ?? string.Empty,
        resp.Director ?? string.Empty,
        resp.Writer ?? string.Empty,
        resp.Actors ?? string.Empty,
        resp.Plot ?? string.Empty,
        resp.Language ?? string.Empty,
        resp.Country ?? string.Empty,
        resp.Awards ?? string.Empty,
        NormalizePoster(resp.Poster),
        resp.ImdbRating ?? string.Empty,
        resp.ImdbVotes ?? string.Empty,
        resp.ImdbID ?? string.Empty,
        resp.Type ?? string.Empty,
        resp.Metascore ?? string.Empty,
        (resp.Ratings ?? new()).Select(r => new RatingDto(r.Source ?? "", r.Value ?? "")).ToList()
        );
    }

    private static string NormalizePoster(string? url)
    => string.IsNullOrWhiteSpace(url) || url.Equals("N/A", StringComparison.OrdinalIgnoreCase)
    ? "/images/no-poster.png"
    : url;
}
