namespace MovieSearchApp.App.State;

using MovieSearchApp.App.Models;

public interface IMovieSearchState
{
    string? Query { get; set; }
    bool IsLoading { get; }
    IReadOnlyList<MovieSearchItem> Results { get; }
    IReadOnlyList<string> Recent { get; }
    MovieDetailsDto? SelectedDetails { get; }
    int Page { get; }
    int TotalResults { get; }
    int PageSize { get; }
    int TotalPages { get; }

    Task InitializeAsync(CancellationToken ct = default);
    Task SearchAsync(int page = 1, CancellationToken ct = default);
    Task SelectHistoryAsync(string query, CancellationToken ct = default);
    Task ClearHistoryAsync(CancellationToken ct = default);
    Task SelectMovieAsync(MovieSearchItem item, CancellationToken ct = default);
    void ClearSelection();
}
