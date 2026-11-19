namespace MovieSearchCase.WebApi.Mappers;

internal static class TvMapper
{
    public static Models.Tv.TvSeries ToApiModel(this Domain.Entities.TvSeries tvSeries) => new()
    {
        Id = tvSeries.Id,
        Name = tvSeries.Name,
        Overview = tvSeries.Overview,
        PosterPath = tvSeries.PosterPath,
        BackdropPath = tvSeries.BackdropPath,
        VoteAverage = tvSeries.VoteAverage,
        FirstAirDate = tvSeries.FirstAirDate,
    };

    public static Models.Tv.TvSeriesPage ToApiModel(this Domain.Entities.TvSeriesPage page) => new()
    {
        Page = page.Page,
        TotalPages = page.TotalPages,
        TotalResults = page.TotalResults,
        Results = page.Results.Select(ToApiModel).ToList(),
    };
}
