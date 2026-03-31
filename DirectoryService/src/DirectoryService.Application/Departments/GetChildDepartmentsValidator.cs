using DirectoryService.Application.Departments.GetDepartment;
using DirectoryService.Application.Validation;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments;

public class GetChildDepartmentsValidator : AbstractValidator<GetChildDepartmentsQuery>
{
    public GetChildDepartmentsValidator()
    {
        RuleFor(q => q.ParentId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("DepartmentId"));

        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.ValueIsInvalid("page"));

        RuleFor(q => q.Size)
            .GreaterThan(0)
            .WithError(GeneralErrors.ValueIsInvalid("size"));
    }
}