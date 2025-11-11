using MovieSearchApp.App.Api;
using MovieSearchApp.App.Services.SearchHistory;
using MovieSearchApp.App.State;
using MovieSearchApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMovieServices(builder.Configuration);
builder.Services.AddScoped<ISearchHistoryService, SearchHistoryService>();
builder.Services.AddScoped<IMovieSearchState, MovieSearchState>();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
// Serve static files from wwwroot (e.g., app.css, images)
app.UseStaticFiles();
app.UseAntiforgery();
// Map framework and referenced library static assets
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapMovieEndpoints();

app.Run();
