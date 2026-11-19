using MovieSearchCase.Shared;
using MovieSearchCase.WebApi.Models.Movies;

namespace MovieSearchCase.WebApi.Handlers.Movies;

public interface IMoviesRequestHandlerFactory
{
    IRequestHandlerAsync GetTrendingMovies();

    // TODO(candidate): add a SearchMovies(string query, int page) method here once you've
    // added the search handler, then wire it up in MoviesRequestHandlerFactory below.
    IRequestHandlerAsync SearchMovies(SearchMoviesRequest request);

    //get movie details by id
    IRequestHandlerAsync GetMovieDetails(int movieId);

    //get movies trailers by id
    IRequestHandlerAsync GetMovieVideos(int movieId);
}
    