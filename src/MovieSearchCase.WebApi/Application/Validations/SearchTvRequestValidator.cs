using FluentValidation;
using MovieSearchCase.WebApi.Models.Tv;

namespace MovieSearchCase.WebApi.Application.Validation;

public sealed class SearchTvRequestValidator : AbstractValidator<SearchTvRequest>
{
    public SearchTvRequestValidator()
    {
        RuleFor(request => request.Query)
            .Must(query => !string.IsNullOrWhiteSpace(query))
            .WithMessage("The 'query' parameter is required.");
        RuleFor(request => request.Page)
            .InclusiveBetween(1, 500)
            .WithMessage("The 'page' parameter must be a whole number from 1 to 500.");
    }
}
