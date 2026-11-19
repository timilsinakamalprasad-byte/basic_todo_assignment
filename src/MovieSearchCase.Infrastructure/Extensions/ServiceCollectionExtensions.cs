using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieSearchCase.Domain.Interfaces.Clients;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.Infrastructure.Cache;
using MovieSearchCase.Infrastructure.Clients;
using MovieSearchCase.Infrastructure.Services;
using MovieSearchCase.Shared.Extensions;

namespace MovieSearchCase.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    private const string HybridCacheSectionName = "HybridCache";

    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IMovieService, MovieService>();
        AddConfiguredDistributedCache(services, configuration);
        services.AddHybridCache(options =>
            configuration.GetSection(HybridCacheSectionName).Bind(options));
        services.AddScoped<HybridCaching>();
        services.AddScoped<ITvService, TvService>();

        return services;
    }

    private static void AddConfiguredDistributedCache(IServiceCollection services, IConfiguration configuration)
    {
        // If the Redis connection string is not configured, fall back to in-memory caching.
        var redisConfiguration =
            configuration[$"{HybridCacheSectionName}:DistributedCache:Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(redisConfiguration))
        {
            services.AddDistributedMemoryCache();
            return;
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfiguration;
            options.InstanceName =
                configuration[$"{HybridCacheSectionName}:DistributedCache:Redis:InstanceName"];
        });
    }

    public static IServiceCollection AddClients(this IServiceCollection services, IConfiguration configuration)
    {
        var tmdbOptions = configuration.GetSection(nameof(TmdbOptions)).Get<TmdbOptions>().ValidateOptions();

        services.AddHttpClient<ITmdbClient, TmdbClient>(client =>
        {
            client.BaseAddress = new Uri(tmdbOptions.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tmdbOptions.AccessToken);
        });

        return services;
    }
}
