using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public record Description
{
    private const int MAX_LENGTH = 1000;

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

        if (normalizedDescription.Length > MAX_LENGTH)
        {
            return Error.Validation(
                "description.value.length",
                "Position description must be less than 1000 characters long.",
                "description.value");
        }

        return new Description(normalizedDescription);
    }
}