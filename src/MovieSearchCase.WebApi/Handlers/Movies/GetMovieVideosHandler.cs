using Microsoft.AspNetCore.Mvc;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.Shared;
using MovieSearchCase.WebApi.Mappers;

namespace MovieSearchCase.WebApi.Handlers.Movies;

public class GetMovieVideosHandler : IRequestHandlerAsync
{
    private readonly IMovieService _movieService;
    private readonly int _id;

    public GetMovieVideosHandler(IMovieService movieService, int id)
    {
        _movieService = movieService;
        _id = id;
    }

    public async Task<IActionResult> HandleAsync(HttpRequest request)
    {
        var videos = await _movieService.GetMovieVideosAsync(_id, request.HttpContext.RequestAborted);
        return new OkObjectResult(videos.Select(video => video.ToApiModel()));
    }
}
