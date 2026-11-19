using FluentAssertions;
using Moq;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Interfaces.Clients;
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
            .Setup(client => client.SearchTvAsync("The Answer", 2, cancellationSource.Token))
            .ReturnsAsync(expectedPage);
        var tvService = new TvService(tmdbClientMock.Object);

        var result = await tvService.SearchTvAsync("The Answer", 2, cancellationSource.Token);

        result.Should().BeSameAs(expectedPage);
        tmdbClientMock.Verify(
            client => client.SearchTvAsync("The Answer", 2, cancellationSource.Token),
            Times.Once);
    }
}
