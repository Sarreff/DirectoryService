using DirectoryService.Application.Departments.GetDepartment;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Departments;

public class GetRootDepartmentsValidator : AbstractValidator<GetRootDepartmentsQuery>
{
    public GetRootDepartmentsValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.ValueIsInvalid("page"));

        RuleFor(q => q.Size)
            .GreaterThan(0)
            .WithError(GeneralErrors.ValueIsInvalid("size"));

        RuleFor(q => q.Prefetch)
            .GreaterThanOrEqualTo(0)
            .WithError(GeneralErrors.ValueIsInvalid("prefetch"));
    }
}