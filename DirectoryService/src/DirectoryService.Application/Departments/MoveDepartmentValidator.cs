using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(c => c.DepartmentId)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentId"));

        RuleFor(c => c.Request)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(c => c)
            .Must(c => c.Request.ParentId is null || c.Request.ParentId.Value != c.DepartmentId)
            .WithError(DepartmentErrors.DepartmentSelfReferenceError());
    }
}