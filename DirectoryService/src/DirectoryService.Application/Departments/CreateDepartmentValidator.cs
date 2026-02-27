using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(c => c.CreateDepartmentRequest)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(c => c.CreateDepartmentRequest.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(c => c.CreateDepartmentRequest.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleForEach(c => c.CreateDepartmentRequest.LocationsId)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("departmentLocations"));

        RuleFor(c => c.CreateDepartmentRequest.LocationsId)
            .Must(ids =>
            {
                var idsList = ids.ToList();
                return idsList.Distinct().Count() == idsList.Count;
            })
            .WithError(GeneralErrors.DuplicatesFound("departmentLocations"));
    }
}