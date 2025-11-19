using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Location.VO;

public record LocationAddress
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

    public static Result<LocationAddress> Create(string country, string city, string street, string houseNumber)
    {
        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<LocationAddress>($"The country cannot be empty.");
        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<LocationAddress>($"The city cannot be empty.");
        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<LocationAddress>($"The street cannot be empty.");
        if (string.IsNullOrWhiteSpace(houseNumber))
            return Result.Failure<LocationAddress>($"The house number cannot be empty.");

        return new LocationAddress(country, city, street, houseNumber);
    }
}