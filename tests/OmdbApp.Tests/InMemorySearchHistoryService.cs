using MovieSearchApp.App.Services.SearchHistory;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OmdbApp.Tests;

// Test-friendly in-memory implementation with same semantics as SearchHistoryService
public sealed class InMemorySearchHistoryService : ISearchHistoryService
{
    private readonly List<string> _items = [];
    private const int MaxItems = 5;

    public Task<IReadOnlyList<string>> GetAsync(CancellationToken ct)
        => Task.FromResult((IReadOnlyList<string>)SearchHistoryCore.Normalize(_items, MaxItems));

    public Task AddAsync(string query, CancellationToken ct)
    {
        SearchHistoryCore.AddOrMoveToFront(_items, query, MaxItems);
        return Task.CompletedTask;
    }

    public Task ClearAsync(CancellationToken ct)
    {
        _items.Clear();
        return Task.CompletedTask;
    }
}