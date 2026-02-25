using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Positions;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        RuleFor(c => c.CreatePositionRequest)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(c => c.CreatePositionRequest.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(c => c.CreatePositionRequest.Description)
            .MustBeValueObject(Description.Create);

        RuleForEach(c => c.CreatePositionRequest.DepartmentsId)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("departmentPositions"));

        RuleFor(c => c.CreatePositionRequest.DepartmentsId)
            .Must(ids =>
            {
                var idsList = ids.ToList();
                return idsList.Distinct().Count() == idsList.Count;
            })
            .WithError(GeneralErrors.DuplicatesFound("departmentPositions"));
    }
}