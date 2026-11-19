using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MovieSearchCase.Domain.Exceptions;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.Shared;
using MovieSearchCase.WebApi.Mappers;
using MovieSearchCase.WebApi.Models.Tv;

namespace MovieSearchCase.WebApi.Handlers.Tv;

public class SearchTvHandler : IRequestHandlerAsync
{
    private readonly ITvService _tvService;
    private readonly IValidator<SearchTvRequest> _validator;
    private readonly SearchTvRequest _request;

    public SearchTvHandler(
        ITvService tvService,
        IValidator<SearchTvRequest> validator,
        SearchTvRequest request)
    {
        _tvService = tvService;
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

        var page = await _tvService.SearchTvAsync(
            _request.Query!.Trim(),
            _request.Page,
            request.HttpContext.RequestAborted);

        return new OkObjectResult(page.ToApiModel());
    }
}
