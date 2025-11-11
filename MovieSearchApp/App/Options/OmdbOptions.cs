namespace MovieSearchApp.App.Options;

public sealed class OmdbOptions
{
    public const string SectionName = "Omdb";
    public string ApiKey { get; set; } = string.Empty;
    // New: allow configuring the API base URL instead of hardcoding it in the client
    public string BaseUrl { get; set; } = "https://www.omdbapi.com";
}