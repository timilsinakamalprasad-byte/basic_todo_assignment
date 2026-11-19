using FluentValidation;
using MovieSearchCase.WebApi.Application.Validation;
using MovieSearchCase.WebApi.Handlers.Movies;
using MovieSearchCase.WebApi.Handlers.Tv;
using MovieSearchCase.WebApi.Models.Movies;
using MovieSearchCase.WebApi.Models.Tv;

namespace MovieSearchCase.WebApi.Handlers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRequestHandlerFactories(this IServiceCollection services)
    {
        services.AddScoped<IValidator<SearchMoviesRequest>, SearchMoviesRequestValidator>();
        services.AddScoped<IValidator<SearchTvRequest>, SearchTvRequestValidator>();
        services.AddScoped<IMoviesRequestHandlerFactory, MoviesRequestHandlerFactory>();
        services.AddScoped<ITvRequestHandlerFactory, TvRequestHandlerFactory>();
        
        return services;
    }
}
