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

    [Fact]
    public async Task GetMovieDetailsAsyncShouldMapMovieAndDetailsFields()
    {
        const string json = """
            {
              "id": 42,
              "title": "The Answer",
              "overview": "A story about the answer.",
              "poster_path": "/poster.jpg",
              "backdrop_path": "/backdrop.jpg",
              "vote_average": 8.4,
              "release_date": "2024-01-15",
              "runtime": 120,
              "tagline": "A great movie",
              "genres": [
                { "name": "Science Fiction" },
                { "name": "Adventure" }
              ]
            }
            """;
        var stub = new StubHttpMessageHandler(json);
        using var httpClient = new HttpClient(stub)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var result = await client.GetMovieDetailsAsync(42, CancellationToken.None);

        stub.RequestUri.Should().Be("https://api.themoviedb.org/3/movie/42");
        result.Id.Should().Be(42);
        result.Title.Should().Be("The Answer");
        result.Overview.Should().Be("A story about the answer.");
        result.ReleaseDate.Should().Be(new DateOnly(2024, 1, 15));
        result.Runtime.Should().Be(120);
        result.Tagline.Should().Be("A great movie");
        result.Genres.Should().BeEquivalentTo("Science Fiction", "Adventure");
    }

    [Fact]
    public async Task GetMovieDetailsAsyncShouldThrowNotFoundWhenResponseIsEmpty()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler("null"))
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var action = () => client.GetMovieDetailsAsync(42, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<MovieException>();
        exception.Which.ErrorCode.Should().Be(ErrorType.EntityNotFound);
        exception.Which.Details.Should().Be("TMDB returned an empty response for movie ID 42.");
    }

    [Fact]
    public async Task GetMovieDetailsAsyncShouldTranslateRequestFailure()
    {
        using var httpClient = new HttpClient(
            new StubHttpMessageHandler(exception: new HttpRequestException("TMDB unavailable")))
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var action = () => client.GetMovieDetailsAsync(42, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<MovieException>();
        exception.Which.ErrorCode.Should().Be(ErrorType.UpstreamServiceUnavailable);
        exception.Which.Details.Should().Be("TMDB did not return details for movie ID 42.");
    }

    [Fact]
    public async Task GetMovieVideosAsyncShouldReturnOnlyYouTubeVideos()
    {
        const string json = """
            {
              "results": [
                {
                  "key": "abc123",
                  "name": "Official Trailer",
                  "site": "YouTube",
                  "type": "Trailer"
                },
                {
                  "key": "def456",
                  "name": "Other-site trailer",
                  "site": "Vimeo",
                  "type": "Trailer"
                },
                {
                  "key": "ghi789",
                  "name": "Teaser",
                  "site": "youtube",
                  "type": "Teaser"
                }
              ]
            }
            """;
        var stub = new StubHttpMessageHandler(json);
        using var httpClient = new HttpClient(stub)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var result = await client.GetMovieVideosAsync(42, CancellationToken.None);

        stub.RequestUri.Should().Be("https://api.themoviedb.org/3/movie/42/videos");
        result.Should().HaveCount(2);
        result.Should().Contain(video =>
            video.Key == "abc123" &&
            video.Name == "Official Trailer" &&
            video.Site == "YouTube" &&
            video.Type == "Trailer");
        result.Should().Contain(video => video.Key == "ghi789" && video.Site == "youtube");
        result.Should().NotContain(video => video.Site == "Vimeo");
    }

    [Fact]
    public async Task GetMovieVideosAsyncShouldReturnEmptyWhenResultsAreNull()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler("""{"results":null}"""))
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var result = await client.GetMovieVideosAsync(42, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMovieVideosAsyncShouldTranslateRequestFailure()
    {
        using var httpClient = new HttpClient(
            new StubHttpMessageHandler(exception: new HttpRequestException("TMDB unavailable")))
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/"),
        };
        var client = new TmdbClient(httpClient);

        var action = () => client.GetMovieVideosAsync(42, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<MovieException>();
        exception.Which.ErrorCode.Should().Be(ErrorType.UpstreamServiceUnavailable);
        exception.Which.Details.Should().Be("TMDB did not return videos for movie ID 42.");
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
