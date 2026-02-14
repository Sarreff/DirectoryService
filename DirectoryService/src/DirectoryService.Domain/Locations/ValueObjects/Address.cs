using CSharpFunctionalExtensions;
using DirectoryService.Shared;

namespace DirectoryService.Domain.Locations.ValueObjects;

public record Address
{
    public string Country { get; }

    public string City { get; }

    public string Street { get; }

    public int Building { get; }

    public int OfficeNumber { get; }

    private Address(string country, string city, string street, int building, int officeNumber)
    {
        Country = country;
        City = city;
        Street = street;
        Building = building;
        OfficeNumber = officeNumber;
    }

    public static Result<Address, Error> Create(
        string country,
        string city,
        string street,
        int building,
        int officeNumber)
    {
        var messages = new List<ErrorMessage>();

        string normalizedCountryAddress = StringNormalization.Normalize(country);
        string normalizedCityAddress = StringNormalization.Normalize(city);
        string normalizedStreetAddress = StringNormalization.Normalize(street);

        if (string.IsNullOrWhiteSpace(normalizedCountryAddress))
        {
            messages.Add(new ErrorMessage(
                "country.address.validation",
                "Country name is not valid",
                "country"));
        }

        if (string.IsNullOrWhiteSpace(normalizedCityAddress))
        {
            messages.Add(new ErrorMessage(
                "city.address.validation",
                "City name is not valid",
                "city"));
        }

        if (string.IsNullOrWhiteSpace(normalizedStreetAddress))
        {
            messages.Add(new ErrorMessage(
                "street.address.validation",
                "Street name is not valid",
                "street"));
        }

        if (building <= 0)
        {
            messages.Add(new ErrorMessage(
                "building.address.validation",
                "Building number is not valid",
                "building"));
        }

        if (officeNumber <= 0)
        {
            messages.Add(new ErrorMessage(
                "office.address.validation",
                "Office number is not valid",
                "office"));
        }

        if (messages.Count > 0)
            return Error.Validation(messages.ToArray());

        return new Address(
            normalizedCountryAddress,
            normalizedCityAddress,
            normalizedStreetAddress,
            building,
            officeNumber);
    }
}