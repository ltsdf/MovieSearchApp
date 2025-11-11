namespace MovieSearchApp.App.Models;

/// <summary>
/// Simple paged result container used by search.
/// Intentionally minimal to avoid overcoupling UI and transport models.
/// </summary>
public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
