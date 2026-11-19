using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Exceptions;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.WebApi.Application.Validation;
using MovieSearchCase.WebApi.Handlers.Movies;
using MovieSearchCase.WebApi.Models.Movies;
using Xunit;
using DomainMovie = MovieSearchCase.Domain.Entities.Movie;
using DomainMoviePage = MovieSearchCase.Domain.Entities.MoviePage;
using ApiMoviePage = MovieSearchCase.WebApi.Models.Movies.MoviePage;

namespace MovieSearchCase.WebApi.Tests.Handlers;

public class SearchMoviesHandlerTests
{
    [Fact]
    public async Task HandleShouldTrimQueryReturnOkAndMapPagedMoviesWhenServiceSucceeds()
    {
        var moviePage = new DomainMoviePage
        {
            Page = 2,
            TotalPages = 3,
            TotalResults = 48,
            Results =
            [
                new DomainMovie
                {
                    Id = 11,
                    Title = "Star Wars",
                    Overview = "A long time ago",
                    VoteAverage = 8.6,
                    ReleaseDate = new DateOnly(1977, 5, 25),
                },
            ],
        };

        using var cancellationSource = new CancellationTokenSource();
        var movieServiceMock = new Mock<IMovieService>();
        movieServiceMock
            .Setup(service => service.SearchMoviesAsync(
                "Star Wars",
                2,
                cancellationSource.Token))
            .ReturnsAsync(moviePage);

        var request = new SearchMoviesRequest
        {
            Query = "  Star Wars  ",
            Page = 2,
        };
        var handler = new SearchMoviesHandler(
            movieServiceMock.Object,
            new SearchMoviesRequestValidator(),
            request);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestAborted = cancellationSource.Token;

        var result = await handler.HandleAsync(httpContext.Request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiPage = okResult.Value.Should().BeOfType<ApiMoviePage>().Subject;
        apiPage.Page.Should().Be(2);
        apiPage.TotalPages.Should().Be(3);
        apiPage.TotalResults.Should().Be(48);
        apiPage.Results.Should().ContainSingle(movie =>
            movie.Id == 11 &&
            movie.Title == "Star Wars" &&
            movie.ReleaseDate == new DateOnly(1977, 5, 25));
        movieServiceMock.Verify(
            service => service.SearchMoviesAsync(
                "Star Wars",
                2,
                cancellationSource.Token),
            Times.Once);
    }

    [Theory]
    [InlineData(null, 1, "The 'query' parameter is required.")]
    [InlineData("   ", 1, "The 'query' parameter is required.")]
    [InlineData("Star Wars", 0, "The 'page' parameter must be a whole number from 1 to 500.")]
    [InlineData("Star Wars", 501, "The 'page' parameter must be a whole number from 1 to 500.")]
    public async Task HandleShouldThrowInvalidRequestAndNotCallServiceWhenRequestIsInvalid(
        string? query,
        int page,
        string expectedDetails)
    {
        var movieServiceMock = new Mock<IMovieService>();
        var request = new SearchMoviesRequest
        {
            Query = query,
            Page = page,
        };
        var handler = new SearchMoviesHandler(
            movieServiceMock.Object,
            new SearchMoviesRequestValidator(),
            request);
        var httpContext = new DefaultHttpContext();

        var action = () => handler.HandleAsync(httpContext.Request);

        var exception = await action.Should().ThrowAsync<InvalidRequestException>();
        exception.Which.Details.Should().Be(expectedDetails);
        movieServiceMock.Verify(
            service => service.SearchMoviesAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleShouldReturnEmptyMappedPageWhenServiceReturnsNoResults()
    {
        var moviePage = new DomainMoviePage
        {
            Page = 0,
            TotalPages = 0,
            TotalResults = 0,
            Results = [],
        };
        var movieServiceMock = new Mock<IMovieService>();
        movieServiceMock
            .Setup(service => service.SearchMoviesAsync(
                "unknown",
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(moviePage);
        var handler = new SearchMoviesHandler(
            movieServiceMock.Object,
            new SearchMoviesRequestValidator(),
            new SearchMoviesRequest
            {
                Query = "unknown",
                Page = 1,
            });
        var httpContext = new DefaultHttpContext();

        var result = await handler.HandleAsync(httpContext.Request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiPage = okResult.Value.Should().BeOfType<ApiMoviePage>().Subject;
        apiPage.Results.Should().BeEmpty();
        apiPage.Page.Should().Be(0);
        apiPage.TotalPages.Should().Be(0);
        apiPage.TotalResults.Should().Be(0);
    }
}
