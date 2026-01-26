using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record Address
{
    public string Value { get; }

    private Address(string value)
    {
        Value = value;
    }

    public static Result<Address, Error> Create(string value)
    {
        string normalizedAddress = StringNormalization.Normalize(value);

        if (string.IsNullOrWhiteSpace(normalizedAddress))
        {
            return GeneralErrors.ValueIsInvalid("Address");
        }

        return new Address(normalizedAddress);
    }
}