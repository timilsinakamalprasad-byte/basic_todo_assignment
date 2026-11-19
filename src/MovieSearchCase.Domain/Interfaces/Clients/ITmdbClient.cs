using MovieSearchCase.Domain.Entities;

namespace MovieSearchCase.Domain.Interfaces.Clients;

/// <summary>
/// Talks to The Movie Database (TMDB) REST API. See https://developer.themoviedb.org/reference.
/// </summary>
public interface ITmdbClient
{
    /// <summary>
    /// Returns this week's trending movies (TMDB's /trending/movie/week endpoint).
    /// </summary>
    Task<IReadOnlyList<Movie>> GetTrendingMoviesAsync(CancellationToken cancellationToken);

    // TODO(candidate): add a method here for TMDB's /search/movie endpoint (query + page)
    // and implement it on TmdbClient, following the same pattern as GetTrendingMoviesAsync.

    /// <summary>
    /// Searches for movies by query and page (TMDB's /search/movie endpoint).
    /// @param query The search query string.
    /// @param page The page number of results to return.
    /// @param cancellationToken The cancellation token.
    /// @returns A MoviePage containing the search results.
    /// </summary>
    Task<MoviePage> SearchMoviesAsync(string query, int page, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the details of a movie by its TMDB ID (TMDB's /movie/{movie_id} endpoint).
    /// @param id The TMDB movie ID.
    /// @param cancellationToken The cancellation token.
    /// @returns A MovieDetails object containing the movie's details.
    /// </summary>
    Task<MovieDetails> GetMovieDetailsAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the videos (trailers, teasers, etc.) for a movie by its TMDB ID (TMDB's /movie/{movie_id}/videos endpoint).
    /// @param id The TMDB movie ID.
    /// @param cancellationToken The cancellation token.
    /// @returns A list of MovieVideo objects containing the movie's videos.
    /// </summary>
    Task<IReadOnlyList<MovieVideo>> GetMovieVideosAsync(int id, CancellationToken cancellationToken);
}
