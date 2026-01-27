using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using TimeZoneConverter;

namespace DirectoryService.Domain.Locations;

public record Timezone
{
    public string Value { get; }

    private Timezone(string value)
    {
        Value = value;
    }

    public static Result<Timezone, Error> Create(string value)
    {
        string normalizedTimezone = StringNormalization.Normalize(value);

        if (string.IsNullOrWhiteSpace(normalizedTimezone)
            || !TZConvert.TryGetTimeZoneInfo(normalizedTimezone, out var timezoneInfo))
        {
            return GeneralErrors.ValueIsInvalid("Timezone");
        }

        return new Timezone(normalizedTimezone);
    }
}