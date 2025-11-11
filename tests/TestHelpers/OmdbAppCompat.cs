using MovieSearchApp.App.Models;

namespace TestHelpers;

// Compatibility layer for tests that expect OmdbApp namespace.
public class OmdbOptions
{
    public string ApiKey { get; set; } = string.Empty;
}

public class OmdbClient
{
    private readonly MovieSearchApp.App.Services.OmdbClient _inner;

    public OmdbClient(HttpClient http, OmdbOptions options)
    {
        var realOptions = new MovieSearchApp.App.Options.OmdbOptions { ApiKey = options.ApiKey };
        _inner = new MovieSearchApp.App.Services.OmdbClient(http, realOptions);
    }

    public Task<PagedResult<MovieSearchItem>> SearchAsync(string query, int page = 1, CancellationToken ct = default)
    => _inner.SearchAsync(query, page, ct);

    public Task<MovieDetailsDto?> GetDetailsAsync(string imdbId, CancellationToken ct = default)
    => _inner.GetDetailsAsync(imdbId, ct);
}
