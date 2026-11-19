using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieSearchCase.Domain.Entities;
using MovieSearchCase.Domain.Exceptions;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.WebApi.Application.Validation;
using MovieSearchCase.WebApi.Handlers.Tv;
using MovieSearchCase.WebApi.Models.Tv;
using Xunit;
using ApiTvSeriesPage = MovieSearchCase.WebApi.Models.Tv.TvSeriesPage;
using DomainTvSeries = MovieSearchCase.Domain.Entities.TvSeries;
using DomainTvSeriesPage = MovieSearchCase.Domain.Entities.TvSeriesPage;

namespace MovieSearchCase.WebApi.Tests.Handlers;

public class SearchTvHandlerTests
{
    [Fact]
    public async Task HandleShouldTrimQueryReturnOkAndMapPagedTvSeries()
    {
        var expectedPage = new DomainTvSeriesPage
        {
            Page = 2,
            TotalPages = 4,
            TotalResults = 72,
            Results =
            [
                new DomainTvSeries
                {
                    Id = 42,
                    Name = "The Answer",
                    Overview = "A series about the answer.",
                    VoteAverage = 8.3,
                    FirstAirDate = new DateOnly(2024, 1, 15),
                },
            ],
        };
        using var cancellationSource = new CancellationTokenSource();
        var tvServiceMock = new Mock<ITvService>();
        tvServiceMock
            .Setup(service => service.SearchTvAsync("The Answer", 2, cancellationSource.Token))
            .ReturnsAsync(expectedPage);
        var handler = new SearchTvHandler(
            tvServiceMock.Object,
            new SearchTvRequestValidator(),
            new SearchTvRequest { Query = " The Answer ", Page = 2 });
        var httpContext = new DefaultHttpContext
        {
            RequestAborted = cancellationSource.Token,
        };

        var result = await handler.HandleAsync(httpContext.Request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiPage = okResult.Value.Should().BeOfType<ApiTvSeriesPage>().Subject;
        apiPage.Page.Should().Be(2);
        apiPage.TotalPages.Should().Be(4);
        apiPage.TotalResults.Should().Be(72);
        apiPage.Results.Should().ContainSingle(series =>
            series.Id == 42 &&
            series.Name == "The Answer" &&
            series.FirstAirDate == new DateOnly(2024, 1, 15));
        tvServiceMock.Verify(
            service => service.SearchTvAsync("The Answer", 2, cancellationSource.Token),
            Times.Once);
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData("  ", 1)]
    [InlineData("The Answer", 0)]
    [InlineData("The Answer", 501)]
    public async Task HandleShouldRejectInvalidQueryOrPage(string? query, int page)
    {
        var tvServiceMock = new Mock<ITvService>();
        var handler = new SearchTvHandler(
            tvServiceMock.Object,
            new SearchTvRequestValidator(),
            new SearchTvRequest { Query = query, Page = page });

        var action = () => handler.HandleAsync(new DefaultHttpContext().Request);

        await action.Should().ThrowAsync<InvalidRequestException>();
        tvServiceMock.Verify(
            service => service.SearchTvAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
