using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Interfaces.Clients;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.Infrastructure.Cache;

namespace MovieSearchCase.Infrastructure.Services;

public class TvService : ITvService
{
    private readonly ITmdbClient _tmdbClient;
    private readonly HybridCaching _hybridCaching;

    public TvService(ITmdbClient tmdbClient, HybridCaching hybridCaching)
    {
        _tmdbClient = tmdbClient;
        _hybridCaching = hybridCaching;
    }

    public async Task<TvSeriesPage> SearchTvAsync(string query, int page, CancellationToken cancellationToken) =>
        await _hybridCaching.GetOrCreateAsync(
            $"tv-search:{page}:{query}",
            token => new ValueTask<TvSeriesPage>(_tmdbClient.SearchTvAsync(query, page, token)),
            cancellationToken: cancellationToken);
}
