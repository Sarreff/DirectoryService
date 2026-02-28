using DirectoryService.Application.Departments.UpdateDepartment;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments;

public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsValidator()
    {
        RuleFor(c => c.UpdateDepartmentLocationsRequest)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleForEach(c => c.UpdateDepartmentLocationsRequest.LocationIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("locationIds"));

        RuleFor(c => c.UpdateDepartmentLocationsRequest.LocationIds)
            .Must(ids =>
            {
                var idsList = ids.ToList();
                return idsList.Distinct().Count() == idsList.Count;
            })
            .WithError(GeneralErrors.DuplicatesFound("locationIds"));
    }
}