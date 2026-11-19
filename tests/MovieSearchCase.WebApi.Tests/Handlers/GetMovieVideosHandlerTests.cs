using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.WebApi.Handlers.Movies;
using Xunit;
using ApiMovieVideo = MovieSearchCase.WebApi.Models.Movies.MovieVideo;

namespace MovieSearchCase.WebApi.Tests.Handlers;

public class GetMovieVideosHandlerTests
{
    [Fact]
    public async Task HandleShouldReturnOkWithMappedVideosAndForwardCancellation()
    {
        IReadOnlyList<MovieVideo> videos =
        [
            new()
            {
                Key = "abc123",
                Name = "Official Trailer",
                Site = "YouTube",
                Type = "Trailer",
            },
        ];
        using var cancellationSource = new CancellationTokenSource();
        var movieServiceMock = new Mock<IMovieService>();
        movieServiceMock
            .Setup(service => service.GetMovieVideosAsync(42, cancellationSource.Token))
            .ReturnsAsync(videos);
        var handler = new GetMovieVideosHandler(movieServiceMock.Object, 42);
        var httpContext = new DefaultHttpContext
        {
            RequestAborted = cancellationSource.Token,
        };

        var result = await handler.HandleAsync(httpContext.Request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiVideos = okResult.Value.Should().BeAssignableTo<IEnumerable<ApiMovieVideo>>().Subject;
        apiVideos.Should().ContainSingle(video =>
            video.Key == "abc123" &&
            video.Name == "Official Trailer" &&
            video.Site == "YouTube" &&
            video.Type == "Trailer");
        movieServiceMock.Verify(
            service => service.GetMovieVideosAsync(42, cancellationSource.Token),
            Times.Once);
    }
}
