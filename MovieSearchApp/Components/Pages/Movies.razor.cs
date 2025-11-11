using Microsoft.AspNetCore.Components;
using MovieSearchApp.App.Models;
using MovieSearchApp.App.State;

namespace MovieSearchApp.Components.Pages;

public partial class Movies : ComponentBase
{
    [Inject] private IMovieSearchState State { get; set; } = default!;

    // Track UI selection/loading state so we can show a spinner while details are fetched
    private string? _selectedImdbId;
    private string? _loadingImdbId;

    // NOTE: We intentionally perform initial history load only after first interactive render.
    // Doing it earlier (during prerender) would throw due to JS-localStorage dependency inside history.
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await State.InitializeAsync();
            StateHasChanged();
        }
    }

    protected async Task OnSubmit() => await State.SearchAsync(1); // new search resets to page1
    protected async Task OnHistorySelected(string q) => await State.SelectHistoryAsync(q); // state resets paging
    protected async Task OnHistoryCleared() => await State.ClearHistoryAsync();

    protected async Task OnMovieSelected(MovieSearchItem item)
    {
        // Toggle close if already selected and not currently loading
        if (_selectedImdbId == item.ImdbId && _loadingImdbId is null && State.SelectedDetails?.ImdbId == item.ImdbId)
        {
            await OnCloseDetails();
            return;
        }

        // Begin loading state for this item
        _selectedImdbId = item.ImdbId;
        _loadingImdbId = item.ImdbId;
        StateHasChanged(); // re-render to show spinner immediately

        await State.SelectMovieAsync(item);
        _loadingImdbId = null; // loaded
        StateHasChanged();
    }

    protected async Task OnPageChanged(int page)
    {
        await State.SearchAsync(page);
    }

    protected Task OnCloseDetails()
    {
        _selectedImdbId = null;
        _loadingImdbId = null;
        State.ClearSelection();
        StateHasChanged();
        return Task.CompletedTask;
    }
}
