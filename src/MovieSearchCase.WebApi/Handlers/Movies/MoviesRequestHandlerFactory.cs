using FluentValidation;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.Shared;
using MovieSearchCase.WebApi.Models.Movies;

namespace MovieSearchCase.WebApi.Handlers.Movies;

public class MoviesRequestHandlerFactory : IMoviesRequestHandlerFactory
{
    private readonly IMovieService _movieService;
    private readonly IValidator<SearchMoviesRequest> _searchMoviesValidator;

    public MoviesRequestHandlerFactory(IMovieService movieService, IValidator<SearchMoviesRequest> searchMoviesValidator)
    {
        _movieService = movieService;
        _searchMoviesValidator = searchMoviesValidator;
    }

    public IRequestHandlerAsync GetTrendingMovies() => new GetTrendingMoviesHandler(_movieService);

    // added the search handler, then wire it up in MoviesController above.
    public IRequestHandlerAsync SearchMovies(SearchMoviesRequest request) =>
        new SearchMoviesHandler(_movieService, _searchMoviesValidator, request);
}
