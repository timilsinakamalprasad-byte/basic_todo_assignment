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

    public static Models.Movies.MovieDetails ToApiModel(this Domain.Entities.MovieDetails movie) => new()
    {
        Id = movie.Id,
        Title = movie.Title,
        Overview = movie.Overview,
        PosterPath = movie.PosterPath,
        BackdropPath = movie.BackdropPath,
        VoteAverage = movie.VoteAverage,
        ReleaseDate = movie.ReleaseDate,
        Runtime = movie.Runtime,
        Tagline = movie.Tagline,
        Genres = movie.Genres,
    };

    public static Models.Movies.MovieVideo ToApiModel(this Domain.Entities.MovieVideo video) => new()
    {
        Key = video.Key,
        Name = video.Name,
        Site = video.Site,
        Type = video.Type,
    };
}
