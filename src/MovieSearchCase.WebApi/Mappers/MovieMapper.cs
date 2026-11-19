namespace MovieSearchCase.WebApi.Mappers;

internal static class MovieMapper
{
    public static Models.Movies.Movie ToApiModel(this Domain.Entities.Movie movie) => new()
    {
        Id = movie.Id,
        Title = movie.Title,
        Overview = movie.Overview,
        PosterPath = movie.PosterPath,
        BackdropPath = movie.BackdropPath,
        VoteAverage = movie.VoteAverage,
        ReleaseDate = movie.ReleaseDate,
    };

    public static Models.Movies.MoviePage ToApiModel(this Domain.Entities.MoviePage page) => new()
    {
        Page = page.Page,
        TotalPages = page.TotalPages,
        TotalResults = page.TotalResults,
        Results = page.Results.Select(ToApiModel).ToList(),
    };
}
