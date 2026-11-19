using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MovieSearchCase.WebApi.Models.Tv;

public record SearchTvRequest
{
    [FromQuery(Name = "query")]
    [SwaggerParameter("TV series name or partial name to search for.", Required = true)]
    public string? Query { get; init; }

    [FromQuery(Name = "page")]
    [SwaggerParameter("Result page, from 1 to 500. Defaults to 1.")]
    public int Page { get; init; } = 1;
}
