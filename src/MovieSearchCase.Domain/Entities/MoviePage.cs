namespace MovieSearchCase.Domain.Entities;

public class MoviePage
{
    public required int Page { get; init; }

    public required int TotalPages { get; init; }

    public required int TotalResults { get; init; }

    public required IReadOnlyList<Movie> Results { get; init; }
}