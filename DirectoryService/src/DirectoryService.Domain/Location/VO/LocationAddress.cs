using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Location.VO;

public sealed record LocationAddress
{
    private LocationAddress(string country, string city, string street, string houseNumber)
    {
        Country = country;
        City = city;
        Street = street;
        HouseNumber = houseNumber;
    }

    public string Country { get; }

    public string City { get; }

    public string Street { get; }

    public string HouseNumber { get; }

    public static Result<LocationAddress, Error> Create(string country, string city, string street, string houseNumber)
    {
        if (string.IsNullOrWhiteSpace(country))
            return GeneralErrors.ValueIsInvalid("country");
        if (string.IsNullOrWhiteSpace(city))
            return GeneralErrors.ValueIsInvalid("city");
        if (string.IsNullOrWhiteSpace(street))
            return GeneralErrors.ValueIsInvalid("street");
        if (string.IsNullOrWhiteSpace(houseNumber))
            return GeneralErrors.ValueIsInvalid("house number");

        return new LocationAddress(country, city, street, houseNumber);
    }
}