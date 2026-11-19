using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Interfaces.Clients;
using MovieSearchCase.Infrastructure.Cache;
using MovieSearchCase.Infrastructure.Services;
using Xunit;

namespace MovieSearchCase.Infrastructure.Tests.Services;

public class TvServiceTests
{
    [Fact]
    public async Task SearchTvAsyncShouldReturnPageAndForwardArguments()
    {
        var expectedPage = new TvSeriesPage
        {
            Page = 2,
            TotalPages = 4,
            TotalResults = 72,
            Results =
            [
                new TvSeries
                {
                    Id = 42,
                    Name = "The Answer",
                    FirstAirDate = new DateOnly(2024, 1, 15),
                },
            ],
        };
        using var cancellationSource = new CancellationTokenSource();
        var tmdbClientMock = new Mock<ITmdbClient>();
        tmdbClientMock
            .Setup(client => client.SearchTvAsync(
                "The Answer",
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPage);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHybridCache();
        using var serviceProvider = services.BuildServiceProvider();
        var tvService = new TvService(
            tmdbClientMock.Object,
            new HybridCaching(serviceProvider.GetRequiredService<HybridCache>()));

        var result = await tvService.SearchTvAsync("The Answer", 2, cancellationSource.Token);
        var cachedResult = await tvService.SearchTvAsync("The Answer", 2, cancellationSource.Token);

        result.Should().BeEquivalentTo(expectedPage);
        cachedResult.Should().BeEquivalentTo(expectedPage);
        tmdbClientMock.Verify(
            client => client.SearchTvAsync(
                "The Answer",
                2,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
