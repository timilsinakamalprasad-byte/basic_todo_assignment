using FluentValidation;
using MovieSearchCase.Domain.Interfaces.Services;
using MovieSearchCase.Shared;
using MovieSearchCase.WebApi.Models.Tv;

namespace MovieSearchCase.WebApi.Handlers.Tv;

public class TvRequestHandlerFactory : ITvRequestHandlerFactory
{
    private readonly ITvService _tvService;
    private readonly IValidator<SearchTvRequest> _searchTvValidator;

    public TvRequestHandlerFactory(ITvService tvService, IValidator<SearchTvRequest> searchTvValidator)
    {
        _tvService = tvService;
        _searchTvValidator = searchTvValidator;
    }

    public IRequestHandlerAsync SearchTv(SearchTvRequest request) =>
        new SearchTvHandler(_tvService, _searchTvValidator, request);
}
