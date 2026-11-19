using System.Net.Http.Json;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Exceptions;
using MovieSearchCase.Domain.Interfaces.Clients;

namespace MovieSearchCase.Infrastructure.Clients;

public class TmdbClient : ITmdbClient
{
    private readonly HttpClient _httpClient;

    public TmdbClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Movie>> GetTrendingMoviesAsync(CancellationToken cancellationToken)
    {
        TmdbPagedResponse<TmdbMovie>? response;

        try
        {
            response = await _httpClient.GetFromJsonAsync<TmdbPagedResponse<TmdbMovie>>(
                "trending/movie/week",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.UpstreamServiceUnavailable,
                "TMDB did not return trending movies.");
        }

        if (response is null)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.UpstreamServiceUnavailable,
                "TMDB returned an empty trending movies response.");
        }

        return response.Results.Select(TmdbMovieMapper.ToDomainModel).ToList();
    }

    // TODO(candidate): implement TMDB's /search/movie?query={query}&page={page} here,
    // following the same try/catch + mapping pattern as GetTrendingMoviesAsync above.

    // See https://developer.themoviedb.org/reference/search-movies for details on the endpoint.
    
    public async Task<MoviePage> SearchMoviesAsync(string query, int page, CancellationToken cancellationToken)
    {
        TmdbPagedResponse<TmdbMovie>? response;

        try
        {
            response = await _httpClient.GetFromJsonAsync<TmdbPagedResponse<TmdbMovie>>(
                $"search/movie?query={Uri.EscapeDataString(query)}&page={page}",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.UpstreamServiceUnavailable,
                "TMDB did not return search results.");
        }

        if (response is null)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.UpstreamServiceUnavailable,
                "TMDB returned an empty search results response.");
        }

        bool hasResults = response.Results.Count > 0; // Check if there are any results in the response, if not return an empty MoviePage with 0 values for Page, TotalPages, and TotalResults.
        
        return new MoviePage
        {
            Page = hasResults ? response.Page : 0,
            TotalPages = hasResults ? response.TotalPages : 0,
            TotalResults = hasResults ? response.TotalResults : 0,
            Results = response.Results.Select(TmdbMovieMapper.ToDomainModel).ToList()
        };
    }

    public async Task<TvSeriesPage> SearchTvAsync(string query, int page, CancellationToken cancellationToken)
    {
        TmdbPagedResponse<TmdbTvSeries>? response;

        try
        {
            response = await _httpClient.GetFromJsonAsync<TmdbPagedResponse<TmdbTvSeries>>(
                $"search/tv?query={Uri.EscapeDataString(query)}&page={page}",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.UpstreamServiceUnavailable,
                "TMDB did not return TV search results.");
        }

        if (response is null)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.UpstreamServiceUnavailable,
                "TMDB returned an empty TV search results response.");
        }

        var hasResults = response.Results.Count > 0;
        return new TvSeriesPage
        {
            Page = hasResults ? response.Page : 0,
            TotalPages = hasResults ? response.TotalPages : 0,
            TotalResults = hasResults ? response.TotalResults : 0,
            Results = response.Results.Select(TmdbMovieMapper.ToDomainModel).ToList(),
        };
    }

    // TODO(candidate): implement TMDB's /movie/{movie_id} endpoint here, following the same try/catch + mapping pattern as GetTrendingMoviesAsync above.
    // See https://developer.themoviedb.org/reference/movie-details for details on the endpoint.
    public async Task<MovieDetails> GetMovieDetailsAsync(int id, CancellationToken cancellationToken)
    {
        TmdbMovieDetails? response;

        try
        {
            response = await _httpClient.GetFromJsonAsync<TmdbMovieDetails>(
                $"movie/{id}",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.UpstreamServiceUnavailable,
                $"TMDB did not return details for movie ID {id}.");
        }

        if (response is null)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.EntityNotFound,
                $"TMDB returned an empty response for movie ID {id}.");
        }

        return TmdbMovieMapper.ToDetailsDomainModel(response);
    }

// TODO(candidate): implement TMDB's /movie/{movie_id}/videos endpoint here, following the same try/catch + mapping pattern as GetTrendingMoviesAsync above.
    // See https://developer.themoviedb.org/reference/movie-videos for details
    public async Task<IReadOnlyList<MovieVideo>> GetMovieVideosAsync(int id, CancellationToken cancellationToken)
    {
        TmdbVideoPagedResponse? response;

        try
        {
            response = await _httpClient.GetFromJsonAsync<TmdbVideoPagedResponse>(
                $"movie/{id}/videos",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new MovieException(   
                MovieException.ExceptionTitle,
                ErrorType.UpstreamServiceUnavailable,
                $"TMDB did not return videos for movie ID {id}.");
        }

        if (response is null)
        {
            throw new MovieException(
                MovieException.ExceptionTitle,
                ErrorType.UpstreamServiceUnavailable,
                $"TMDB returned an empty response for videos of movie ID {id}.");
        }

         // Only videos with site "YouTube" are considered valid, as per the requirements. Filter out any videos that do not have site "YouTube".
        return response.Results?
            .Where(video => video.Site.Equals("YouTube", StringComparison.OrdinalIgnoreCase))
            .Select(TmdbMovieMapper.ToDomainModel)
            .ToList() ?? new List<MovieVideo>();
    }
    
}
