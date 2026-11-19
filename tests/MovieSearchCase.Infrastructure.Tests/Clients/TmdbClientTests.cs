using System.Net;
using System.Text;
using FluentAssertions;
using MovieSearchCase.Domain.Exceptions;
using MovieSearchCase.Infrastructure.Clients;
using Xunit;

namespace MovieSearchCase.Infrastructure.Tests.Clients;

public class TmdbClientTests
{
    [Fact]
    public async Task SearchMoviesAsyncShouldEscapeQueryAndMapResponse()
    {
        const string json = """
            {
              "page": 2,
              "results": [
                {
                  "id": 11,
                  "title": "Star Wars",
                  "overview": "A long time ago",
                  "poster_path": "/poster.jpg",
                  "backdrop_path": "/backdrop.jpg",
                  "vote_average": 8.6,
                  "release_date": "1977-05-25"
                }
              ],
              "total_pages": 3,
              "total_results": 48
            }
            """;
        var stub = new StubHttpMessageHandler(json);
        using var httpClient = new HttpClient(stub)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var result = await client.SearchMoviesAsync(
            "Star & Wars",
            2,
            CancellationToken.None);

        stub.RequestUri.Should().Be(
            "https://api.themoviedb.org/3/search/movie?query=Star %26 Wars&page=2");
        result.Page.Should().Be(2);
        result.TotalPages.Should().Be(3);
        result.TotalResults.Should().Be(48);
        result.Results.Should().ContainSingle(movie =>
            movie.Id == 11 &&
            movie.Title == "Star Wars" &&
            movie.ReleaseDate == new DateOnly(1977, 5, 25));
    }

    [Fact]
    public async Task SearchMoviesAsyncShouldReturnZeroPaginationWhenResultsAreEmpty()
    {
        const string json = """
            {
              "page": 1,
              "results": [],
              "total_pages": 1,
              "total_results": 0
            }
            """;
        using var httpClient = new HttpClient(new StubHttpMessageHandler(json))
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var result = await client.SearchMoviesAsync(
            "unknown",
            1,
            CancellationToken.None);

        result.Page.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.TotalResults.Should().Be(0);
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchMoviesAsyncShouldThrowMovieExceptionWhenRequestFails()
    {
        using var httpClient = new HttpClient(
            new StubHttpMessageHandler(exception: new HttpRequestException("TMDB unavailable")))
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var action = () => client.SearchMoviesAsync(
            "Star Wars",
            1,
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<MovieException>();
        exception.Which.ErrorCode.Should().Be(ErrorType.UpstreamServiceUnavailable);
        exception.Which.Details.Should().Be("TMDB did not return search results.");
    }

    [Fact]
    public async Task SearchMoviesAsyncShouldThrowMovieExceptionWhenResponseIsEmpty()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler("null"))
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var action = () => client.SearchMoviesAsync(
            "Star Wars",
            1,
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<MovieException>();
        exception.Which.ErrorCode.Should().Be(ErrorType.UpstreamServiceUnavailable);
        exception.Which.Details.Should().Be("TMDB returned an empty search results response.");
    }

    private sealed class StubHttpMessageHandler(
        string? json = null,
        HttpRequestException? exception = null) : HttpMessageHandler
    {
        public string? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri?.ToString();

            if (exception is not null)
            {
                throw exception;
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json ?? "{}", Encoding.UTF8, "application/json"),
                RequestMessage = request,
            });
        }
    }
}
