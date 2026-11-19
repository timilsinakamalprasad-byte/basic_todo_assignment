using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MovieSearchCase.WebApi.Application.Versioning;
using MovieSearchCase.WebApi.Handlers.Tv;
using Swashbuckle.AspNetCore.Annotations;

namespace MovieSearchCase.WebApi.Controllers.V1;

[ApiVersion("1.0")]
[VersionedRoute]
public class TvController : ApiControllerBase
{
    private readonly ITvRequestHandlerFactory _requestHandlerFactory;

    public TvController(ITvRequestHandlerFactory requestHandlerFactory)
    {
        _requestHandlerFactory = requestHandlerFactory;
    }

    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search TV series", OperationId = "SearchTv")]
    [SwaggerResponse(StatusCodes.Status200OK, "Matching TV series with pagination", typeof(Models.Tv.TvSeriesPage))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid query or page parameter")]
    [SwaggerResponse(StatusCodes.Status503ServiceUnavailable, "TMDB unavailable")]
    public async Task<IActionResult> Search([AsParameters] Models.Tv.SearchTvRequest request) =>
        await _requestHandlerFactory.SearchTv(request).HandleAsync(Request);
}
