namespace MovieSearchApp.App.Models;

public sealed record MovieDetailsDto(
    string Title,
    string Year,
    string Rated,
    string Released,
    string Runtime,
    string Genre,
    string Director,
    string Writer,
    string Actors,
    string Plot,
    string Language,
    string Country,
    string Awards,
    string PosterUrl,
    string ImdbRating,
    string ImdbVotes,
    string ImdbId,
    string Type,
    string Metascore,
 IReadOnlyList<RatingDto> Ratings
);
