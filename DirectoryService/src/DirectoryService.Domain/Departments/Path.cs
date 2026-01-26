using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public partial record Path
{
    public string Value { get; }

    private Path(string value)
    {
        Value = value;
    }

    public static Result<Path, Error> Create(string value)
    {
        string normalizedPath = StringNormalization.Normalize(value);

        if (string.IsNullOrWhiteSpace(normalizedPath))
        {
            return GeneralErrors.ValueIsInvalid("Path");
        }

        return new Path(normalizedPath);
    }
}