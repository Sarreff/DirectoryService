using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Positions.ValueObjects;

public record Description
{
    public string? Value { get; }

    private Description(string? value)
    {
        Value = value;
    }

    public static Result<Description, Error> Create(string? value)
    {
        if (value is null)
        {
            return new Description(value);
        }

        string normalizedDescription = StringNormalization.Normalize(value);

        if (string.IsNullOrWhiteSpace(normalizedDescription))
        {
            return GeneralErrors.ValueIsInvalid("Description");
        }

        if (normalizedDescription.Length > LengthConstants.LENGTH1000)
        {
            return Error.Validation(
                "description.value.length",
                "Position description must be less than 1000 characters long.",
                "description.value");
        }

        return new Description(normalizedDescription);
    }
}