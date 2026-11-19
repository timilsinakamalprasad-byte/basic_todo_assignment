using MovieSearchCase.Domain.Entities;

namespace MovieSearchCase.Domain.Interfaces.Services;

public interface ITvService
{
    Task<TvSeriesPage> SearchTvAsync(string query, int page, CancellationToken cancellationToken);
}
