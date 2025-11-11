namespace MovieSearchApp.App.Services.SearchHistory;

public interface ISearchHistoryService
{
    Task<IReadOnlyList<string>> GetAsync(CancellationToken ct = default);
    Task AddAsync(string query, CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
}