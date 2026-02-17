using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations.ValueObjects;

public sealed record Name
{
    public string Value { get; }

    private Name(string value)
    {
        Value = value;
    }

    public static Result<Name, Error> Create(string value)
    {
        string normalizedName = StringNormalization.Normalize(value);

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return GeneralErrors.ValueIsInvalid("Name");
        }

        if (normalizedName.Length is < LengthConstants.LENGTH3 or > LengthConstants.LENGTH150)
        {
            return Error.Validation(
                "name.value.length",
                "Location name must be between 3 and 120 characters long.",
                "name.value");
        }

        return new Name(normalizedName);
    }
}