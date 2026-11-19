using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MovieSearchCase.Domain.Exceptions;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.Shared;
using MovieSearchCase.WebApi.Mappers;
using MovieSearchCase.WebApi.Models.Movies;

namespace MovieSearchCase.WebApi.Handlers.Movies;

public class SearchMoviesHandler : IRequestHandlerAsync
{
    private readonly IMovieService _movieService;
    private readonly IValidator<SearchMoviesRequest> _validator;
    private readonly SearchMoviesRequest _request;

    public SearchMoviesHandler(
        IMovieService movieService,
        IValidator<SearchMoviesRequest> validator,
        SearchMoviesRequest request)
    {
        _movieService = movieService;
        _validator = validator;
        _request = request;
    }

    public async Task<IActionResult> HandleAsync(HttpRequest request)
    {
        var validationResult = await _validator.ValidateAsync(
            _request,
            request.HttpContext.RequestAborted);
        if (!validationResult.IsValid)
        {
            throw new InvalidRequestException(validationResult.Errors[0].ErrorMessage);
        }

        var page = await _movieService.SearchMoviesAsync(
            _request.Query!.Trim(),
            _request.Page,
            request.HttpContext.RequestAborted);

        return new OkObjectResult(page.ToApiModel());
    }
}
