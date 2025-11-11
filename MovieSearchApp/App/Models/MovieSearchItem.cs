namespace MovieSearchApp.App.Models;

public sealed record MovieSearchItem(
    string Title,
    string Year,
    string ImdbId,
    string Type,
    string PosterUrl
);
