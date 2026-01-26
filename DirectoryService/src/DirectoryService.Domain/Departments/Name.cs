using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record Name
{
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 150;

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

        if (normalizedName.Length is < MIN_LENGTH or > MAX_LENGTH)
        {
            return Error.Validation(
                "name.value.length",
                "Department name must be between 3 and 150 characters long.",
                "name.value");
        }

        return new Name(normalizedName);
    }
}