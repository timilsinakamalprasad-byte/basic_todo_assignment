using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Interfaces.Clients;
using MovieSearchCase.Domain.Interfaces.Services;

namespace MovieSearchCase.Infrastructure.Services;

public class TvService : ITvService
{
    private readonly ITmdbClient _tmdbClient;

    public TvService(ITmdbClient tmdbClient)
    {
        _tmdbClient = tmdbClient;
    }

    public Task<TvSeriesPage> SearchTvAsync(string query, int page, CancellationToken cancellationToken) =>
        _tmdbClient.SearchTvAsync(query, page, cancellationToken);
}
