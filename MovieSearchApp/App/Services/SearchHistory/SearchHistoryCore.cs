namespace MovieSearchApp.App.Services.SearchHistory;

/// <summary>
/// Shared list manipulation logic for search history.
/// Keeps newest queries at the front, removes duplicates (case-insensitive), and trims to a maximum size.
/// </summary>
public static class SearchHistoryCore
{
    public static void AddOrMoveToFront(List<string> list, string query, int maxItems)
    {
        if (string.IsNullOrWhiteSpace(query)) return;
        list.RemoveAll(s => string.Equals(s, query, StringComparison.OrdinalIgnoreCase));
        list.Insert(0, query);
        Trim(list, maxItems);
    }

    public static IReadOnlyList<string> Normalize(IEnumerable<string> source, int maxItems)
    {
        var result = source
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Take(maxItems)
        .ToList();
        return result;
    }

    private static void Trim(List<string> list, int maxItems)
    {
        if (list.Count > maxItems)
            list.RemoveRange(maxItems, list.Count - maxItems);
    }
}
