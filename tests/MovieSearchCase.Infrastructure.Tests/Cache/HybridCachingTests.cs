using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieSearchCase.Infrastructure.Cache;
using MovieSearchCase.Infrastructure.Extensions;
using Xunit;

namespace MovieSearchCase.Infrastructure.Tests.Cache;

public class HybridCachingTests
{
    [Fact]
public async Task GetOrCreateAsyncShouldReuseCachedValuesAndSeparateKeys()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHybridCache();
        using var serviceProvider = services.BuildServiceProvider();
    var caching = new HybridCaching(serviceProvider.GetRequiredService<HybridCache>());
    var factoryCalls = 0;
    ValueTask<int> Factory(CancellationToken _) => ValueTask.FromResult(++factoryCalls);

    var firstResult = await caching.GetOrCreateAsync("tv-search:1:The Answer", Factory);
    var cachedResult = await caching.GetOrCreateAsync("tv-search:1:The Answer", Factory);
    var otherPage = await caching.GetOrCreateAsync("tv-search:2:The Answer", Factory);
    var otherQuery = await caching.GetOrCreateAsync("tv-search:1:Another Series", Factory);

    cachedResult.Should().Be(firstResult);
    otherPage.Should().Be(2);
    otherQuery.Should().Be(3);
    factoryCalls.Should().Be(3);
    }

[Fact]
public async Task GetOrCreateAsyncShouldShareFactoryCallForConcurrentRequests()
    {
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddHybridCache();
    using var serviceProvider = services.BuildServiceProvider();
    var caching = new HybridCaching(serviceProvider.GetRequiredService<HybridCache>());
    var factoryCalls = 0;

    async ValueTask<int> Factory(CancellationToken cancellationToken)
        {
        Interlocked.Increment(ref factoryCalls);
        await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
        return 42;
        }

    var requests = Enumerable.Range(0, 5)
        .Select(_ => caching.GetOrCreateAsync("tv-search:1:The Answer", Factory).AsTask());

    var results = await Task.WhenAll(requests);

    results.Should().OnlyContain(result => result == 42);
    factoryCalls.Should().Be(1);
    }

    [Fact]
    public void AddServicesShouldBindHybridCacheOptionsFromConfiguration()
    {
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HybridCache:DefaultEntryOptions:Expiration"] = "00:07:00",
            ["HybridCache:DefaultEntryOptions:LocalCacheExpiration"] = "00:02:00",
        })
        .Build();
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddServices(configuration);
    using var serviceProvider = services.BuildServiceProvider();

    var options = serviceProvider.GetRequiredService<IOptions<HybridCacheOptions>>().Value;

    options.DefaultEntryOptions.Should().NotBeNull();
    options.DefaultEntryOptions!.Expiration.Should().Be(TimeSpan.FromMinutes(7));
    options.DefaultEntryOptions.LocalCacheExpiration.Should().Be(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public void AddServicesShouldRegisterRedisWhenConfigured()
    {
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["HybridCache:DistributedCache:Redis:ConnectionString"] = "localhost:6379,abortConnect=false",
            ["HybridCache:DistributedCache:Redis:InstanceName"] = "MovieSearch:",
        })
        .Build();
    var services = new ServiceCollection();

    services.AddServices(configuration);

    services.Any(descriptor => descriptor.ServiceType == typeof(IDistributedCache))
        .Should()
        .BeTrue();
    }

    [Fact]
    public void AddServicesShouldUseLocalCacheWhenRedisConnectionStringIsMissing()
    {
    var configuration = new ConfigurationBuilder().Build();
    var services = new ServiceCollection();

    services.AddServices(configuration);

    services.Any(descriptor =>
            descriptor.ServiceType == typeof(IDistributedCache) &&
            descriptor.ImplementationType == typeof(MemoryDistributedCache))
        .Should()
        .BeTrue();
    }
}
