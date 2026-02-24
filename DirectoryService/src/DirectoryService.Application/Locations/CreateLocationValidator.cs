using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(c => c.CreateLocationRequest)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("request"));

        RuleFor(c => c.CreateLocationRequest.Name)
            .MustBeValueObject(Name.Create);

        RuleFor(c => c.CreateLocationRequest.AddressDto)
            .MustBeValueObject(addr => Address.Create(
                addr.AddressCountry,
                addr.AddressCity,
                addr.AddressStreet,
                addr.AddressBuilding,
                addr.AddressOfficeNumber));

        RuleFor(c => c.CreateLocationRequest.Timezone)
            .MustBeValueObject(Timezone.Create);
    }
}