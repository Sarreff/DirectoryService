using DirectoryService.Application.Locations.GetLocation;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations;

public class GetLocationValidator : AbstractValidator<GetLocationsQuery>
{
    public GetLocationValidator()
    {
        RuleFor(q => q.GetLocationsRequest)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(q => q.GetLocationsRequest.SearchName)
            .MaximumLength(100)
            .WithError(GeneralErrors.ValueIsInvalid("searchName"));

        RuleFor(q => q.GetLocationsRequest.SortBy)
            .Must(x => x is null or "name" or "date")
            .WithError(GeneralErrors.ValueIsInvalid("sortBy"));

        RuleFor(q => q.GetLocationsRequest.SortByOrder)
            .Must(x => x is null or "asc" or "desc")
            .WithError(GeneralErrors.ValueIsInvalid("sortByOrder"));

        RuleFor(q => q.GetLocationsRequest.Page)
            .NotNull()
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.ValueIsInvalid("page"));

        RuleFor(q => q.GetLocationsRequest.PageSize)
            .NotNull()
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.ValueIsInvalid("pageSize"));
    }
}