using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public partial record Identifier
{
    private const int MIN_LENGTH = 3;
    private const int MAX_LENGTH = 150;

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

        if (normalizedIdentifier.Length is < MIN_LENGTH or > MAX_LENGTH)
        {
            messages.Add(new ErrorMessage(
                "identifier.value.length",
                "Department identifier must be between 3 and 150 characters long.",
                "identifier.value"));
        }

        if (MyRegex().IsMatch(normalizedIdentifier))
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

    [GeneratedRegex(@"^[A-Za-z\s-]+$")]
    private static partial Regex MyRegex();
}