using Microsoft.JSInterop;

namespace MovieSearchApp.App.Services.SearchHistory;

/// <summary>
/// Persists recent search queries to browser localStorage via JS interop.
/// Uses <see cref="SearchHistoryCore"/> for list semantics (dedupe, trim, ordering).
/// </summary>
public sealed class SearchHistoryService(IJSRuntime js) : ISearchHistoryService
{
    private readonly IJSRuntime _js = js;
    internal const string Key = "omdbapp:recentQueries";
    private const int MaxItems = 5;

    public async Task<IReadOnlyList<string>> GetAsync(CancellationToken ct = default)
    {
        // Fetch raw JSON; during prerender JS isn't available -> return empty gracefully
        string? json;
        try
        {
            json = await _js.InvokeAsync<string?>("localStorage.getItem", ct, Key);
        } catch (InvalidOperationException)
        {
            return Array.Empty<string>();
        } catch
        {
            return Array.Empty<string>();
        }

        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<string>();

        List<string> list;
        try
        {
            list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? [];
        } catch
        {
            return Array.Empty<string>();
        }

        return SearchHistoryCore.Normalize(list, MaxItems);
    }

    public async Task AddAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return;

        var current = (await GetAsync(ct)).ToList();
        SearchHistoryCore.AddOrMoveToFront(current, query, MaxItems);

        var json = System.Text.Json.JsonSerializer.Serialize(current);
        // Use InvokeAsync<object?> because tests mock the underlying generic call instead of the extension method
        await _js.InvokeAsync<object?>("localStorage.setItem", ct, Key, json);
    }

    public Task ClearAsync(CancellationToken ct = default)
    // Use InvokeAsync<object?> to align with test mock and avoid IJSVoidResult cast issues
    => _js.InvokeAsync<object?>("localStorage.removeItem", ct, Key).AsTask();
}
