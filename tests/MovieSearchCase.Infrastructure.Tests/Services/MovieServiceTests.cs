using FluentAssertions;
using Moq;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Interfaces.Clients;
using MovieSearchCase.Infrastructure.Services;
using Xunit;

namespace MovieSearchCase.Infrastructure.Tests.Services;

public class MovieServiceTests
{
    [Fact]
    public async Task GetTrendingAsyncShouldReturnMoviesFromTmdbClient()
    {
        var expectedMovies = new List<Movie>
        {
            new()
            {
                Id = 42,
                Title = "The Answer",
                VoteAverage = 9.2,
            },
        };

        var tmdbClientMock = new Mock<ITmdbClient>();
        tmdbClientMock
            .Setup(client => client.GetTrendingMoviesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMovies);

        var movieService = new MovieService(tmdbClientMock.Object);

        var result = await movieService.GetTrendingAsync(CancellationToken.None);

        result.Should().BeEquivalentTo(expectedMovies);
    }

    [Fact]
    public async Task SearchMoviesAsyncShouldReturnPageAndForwardSearchArguments()
    {
        var expectedPage = new MoviePage
        {
            Page = 2,
            TotalPages = 5,
            TotalResults = 100,
            Results =
            [
                new Movie
                {
                    Id = 42,
                    Title = "The Answer",
                    VoteAverage = 9.2,
                },
            ],
        };

        using var cancellationSource = new CancellationTokenSource();
        var tmdbClientMock = new Mock<ITmdbClient>();
        tmdbClientMock
            .Setup(client => client.SearchMoviesAsync(
                "Star Wars",
                2,
                cancellationSource.Token))
            .ReturnsAsync(expectedPage);

        var movieService = new MovieService(tmdbClientMock.Object);

        var result = await movieService.SearchMoviesAsync(
            "Star Wars",
            2,
            cancellationSource.Token);

        result.Should().BeSameAs(expectedPage);
        tmdbClientMock.Verify(
            client => client.SearchMoviesAsync(
                "Star Wars",
                2,
                cancellationSource.Token),
            Times.Once);
    }
}
