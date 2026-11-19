using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.WebApi.Handlers.Movies;
using Xunit;
using ApiMovieDetails = MovieSearchCase.WebApi.Models.Movies.MovieDetails;

namespace MovieSearchCase.WebApi.Tests.Handlers;

public class GetMovieDetailsHandlerTests
{
    [Fact]
    public async Task HandleShouldReturnOkWithMappedDetailsAndForwardCancellation()
    {
        var details = new MovieDetails
        {
            Id = 42,
            Title = "The Answer",
            Overview = "A story about the answer.",
            Runtime = 120,
            Tagline = "A great movie",
            Genres = ["Science Fiction", "Adventure"],
        };
        using var cancellationSource = new CancellationTokenSource();
        var movieServiceMock = new Mock<IMovieService>();
        movieServiceMock
            .Setup(service => service.GetMovieDetailsAsync(42, cancellationSource.Token))
            .ReturnsAsync(details);
        var handler = new GetMovieDetailsHandler(movieServiceMock.Object, 42);
        var httpContext = new DefaultHttpContext
        {
            RequestAborted = cancellationSource.Token,
        };

        var result = await handler.HandleAsync(httpContext.Request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiDetails = okResult.Value.Should().BeOfType<ApiMovieDetails>().Subject;
        apiDetails.Id.Should().Be(42);
        apiDetails.Title.Should().Be("The Answer");
        apiDetails.Runtime.Should().Be(120);
        apiDetails.Tagline.Should().Be("A great movie");
        apiDetails.Genres.Should().BeEquivalentTo("Science Fiction", "Adventure");
        movieServiceMock.Verify(
            service => service.GetMovieDetailsAsync(42, cancellationSource.Token),
            Times.Once);
    }
}
