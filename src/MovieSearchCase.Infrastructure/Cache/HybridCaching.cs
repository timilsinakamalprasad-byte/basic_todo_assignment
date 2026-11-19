using Microsoft.Extensions.Caching.Hybrid;

namespace MovieSearchCase.Infrastructure.Cache;

public sealed class HybridCaching
{
    private readonly HybridCache _hybridCache;

    public HybridCaching(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    public ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        CancellationToken cancellationToken = default,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null)
        => _hybridCache.GetOrCreateAsync(
            key,
            factory,
            options,
            tags,
            cancellationToken);
}
