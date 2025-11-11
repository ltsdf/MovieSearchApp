using MovieSearchApp.App.Options;
using MovieSearchApp.App.Services;

namespace MovieSearchApp.App.Api;

public static class MovieApi
{
    public static IServiceCollection AddMovieServices(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new OmdbOptions();
        configuration.GetSection(OmdbOptions.SectionName).Bind(options);
        services.AddSingleton(options);

        services.AddHttpClient<IOmdbClient, OmdbClient>();
        return services;
    }

    public static IEndpointRouteBuilder MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movies");

        group.MapGet("/search", async (string? q, int page, IOmdbClient client, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest("Query 'q' is required.");

            var results = await client.SearchAsync(q, page <= 0 ? 1 : page, ct);
            return Results.Ok(results);
        });

        group.MapGet("/details/{imdbId}", async (string imdbId, IOmdbClient client, CancellationToken ct) =>
        {
            var details = await client.GetDetailsAsync(imdbId, ct);
            return details is null ? Results.NotFound() : Results.Ok(details);
        });

        return app;
    }
}
