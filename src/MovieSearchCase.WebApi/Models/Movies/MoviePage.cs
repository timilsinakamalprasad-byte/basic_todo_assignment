namespace MovieSearchCase.WebApi.Models.Movies;

/// <summary>
/// API response shape for a movie search. Deliberately separate from
/// <see cref="Domain.Entities.MoviePage"/> so the wire format can evolve independently
/// of the domain model.
/// </summary>
public record MoviePage
{
    public required int Page { get; init; }

    public required int TotalPages { get; init; }

    public required int TotalResults { get; init; }

    public required IReadOnlyList<Movie> Results { get; init; }
}