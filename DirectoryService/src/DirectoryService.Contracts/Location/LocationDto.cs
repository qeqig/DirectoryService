namespace DirectoryService.Contracts.Location;

public record LocationDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string Country { get; init; } = null!;

    public string City { get; init; } = null!;

    public string Street { get; init; } = null!;

    public string HouseNumber { get; init; } = null!;

    public string Timezone { get; init; } = null!;

    public bool IsActive { get; init; }

    public DateTime Created { get; init; }

    public DateTime Updated { get; init; }
}