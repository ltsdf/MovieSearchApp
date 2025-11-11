using MovieSearchApp.App.Models;
using MovieSearchApp.App.Services;
using MovieSearchApp.App.Services.SearchHistory;

namespace MovieSearchApp.App.State;

public sealed class MovieSearchState(IOmdbClient client, ISearchHistoryService history) : IMovieSearchState
{
    private readonly IOmdbClient _client = client; // note: client already encapsulates pagination page size
    private readonly ISearchHistoryService _history = history;

    public string? Query { get; set; }
    public bool IsLoading { get; private set; }
    public IReadOnlyList<MovieSearchItem> Results { get; private set; } = [];
    public IReadOnlyList<string> Recent { get; private set; } = [];
    public MovieDetailsDto? SelectedDetails { get; private set; }
    public int Page { get; private set; } = 1;
    public int TotalResults { get; private set; }
    public int PageSize { get; private set; } = 10; // aligned with OMDb
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalResults / PageSize);

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        Recent = await _history.GetAsync(ct);
    }

    public async Task SearchAsync(int page = 1, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(Query)) return;

        IsLoading = true;
        SelectedDetails = null;
        try
        {
            var paged = await _client.SearchAsync(Query!, page, ct);
            Results = paged.Items;
            Page = paged.Page;
            PageSize = paged.PageSize;
            TotalResults = paged.TotalCount;
            if (page == 1)
            {
                await _history.AddAsync(Query!, ct);
                Recent = await _history.GetAsync(ct);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SelectHistoryAsync(string query, CancellationToken ct = default)
    {
        Query = query;
        await SearchAsync(1, ct); // always restart at first page when choosing history
    }

    public async Task ClearHistoryAsync(CancellationToken ct = default)
    {
        await _history.ClearAsync(ct);
        Recent = Array.Empty<string>();
    }

    public async Task SelectMovieAsync(MovieSearchItem item, CancellationToken ct = default)
    {
        SelectedDetails = await _client.GetDetailsAsync(item.ImdbId, ct);
    }

    public void ClearSelection()
    {
        SelectedDetails = null;
    }
}
