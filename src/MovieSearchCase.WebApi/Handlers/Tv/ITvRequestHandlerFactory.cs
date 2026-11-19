using MovieSearchCase.Shared;
using MovieSearchCase.WebApi.Models.Tv;

namespace MovieSearchCase.WebApi.Handlers.Tv;

public interface ITvRequestHandlerFactory
{
    IRequestHandlerAsync SearchTv(SearchTvRequest request);
}
