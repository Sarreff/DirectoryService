using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Departments.ValueObjects;

public sealed record Identifier
{
    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier, Error> Create(string value)
    {
        var messages = new List<ErrorMessage>();

        string normalizedIdentifier = StringNormalization.Normalize(value);

        if (string.IsNullOrWhiteSpace(normalizedIdentifier))
        {
            messages.Add(new ErrorMessage(
                "identifier.value",
                "Department identifier cannot be null or empty.",
                "identifier.value"));
        }

        if (normalizedIdentifier.Length is < LengthConstants.LENGTH3 or > LengthConstants.LENGTH150)
        {
            messages.Add(new ErrorMessage(
                "identifier.value.length",
                "Department identifier must be between 3 and 150 characters long.",
                "identifier.value"));
        }

        if (!IdentifierRegex.LatinName().IsMatch(normalizedIdentifier))
        {
            messages.Add(new ErrorMessage(
                "identifier.name",
                "Identifier name must contain only the Latin alphabet.",
                "identifier.name"));
        }

        if (messages.Count > 0)
            return Error.Validation(messages.ToArray());

        return new Identifier(normalizedIdentifier);
    }
}