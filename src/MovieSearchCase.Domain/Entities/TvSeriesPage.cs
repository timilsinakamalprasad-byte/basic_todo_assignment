namespace MovieSearchCase.Domain.Entities;

public class TvSeriesPage
{
    public required int Page { get; init; }

    public required int TotalPages { get; init; }

    public required int TotalResults { get; init; }

    public required IReadOnlyList<TvSeries> Results { get; init; }
}
