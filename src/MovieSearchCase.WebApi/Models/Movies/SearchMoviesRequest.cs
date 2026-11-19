using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MovieSearchCase.WebApi.Models.Movies;

public record SearchMoviesRequest
{
    [FromQuery(Name = "query")]
    [SwaggerParameter("Movie title or partial title to search for.", Required = true)]
    public string? Query { get; init; }

    [FromQuery(Name = "page")]
    [SwaggerParameter("Result page, from 1 to 500. Defaults to 1.")]
    public int Page { get; init; } = 1;
}
