namespace MovieSearchCase.WebApi.Models.Movies;

public record MovieDetails : Movie
{
    public int? Runtime { get; init; }

    public string? Tagline { get; init; }

    public required IReadOnlyList<string> Genres { get; init; }
}
