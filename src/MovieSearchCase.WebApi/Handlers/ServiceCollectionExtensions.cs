using FluentValidation;
using MovieSearchCase.WebApi.Application.Validation;
using MovieSearchCase.WebApi.Handlers.Movies;
using MovieSearchCase.WebApi.Models.Movies;

namespace MovieSearchCase.WebApi.Handlers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRequestHandlerFactories(this IServiceCollection services)
    {
        services.AddScoped<IValidator<SearchMoviesRequest>, SearchMoviesRequestValidator>();
        services.AddScoped<IMoviesRequestHandlerFactory, MoviesRequestHandlerFactory>();
        
        return services;
    }
}
