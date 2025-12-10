using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Location.VO;

namespace DirectoryService.Domain.Location;

public sealed class Location : Entity<LocationId>
{
    // EF core
    private Location(LocationId id)
        : base(id) { }

    private List<DepartmentLocation> _departments = [];

    private Location(
        LocationId id,
        LocationName locationName,
        LocationAddress address,
        LocationTimezone locationTimezone
        /*IEnumerable<DepartmentLocations> departments*/)
        : base(id)
    {
        LocationName = locationName;
        Address = address;
        LocationTimezone = locationTimezone;
        //_departments = departments.ToList();
    }

    public LocationName LocationName { get; private set; } = null!;

    public LocationAddress Address { get; private set; } = null!;

    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public LocationTimezone LocationTimezone { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public static Result<Location> Create(
        LocationName locationName,
        LocationTimezone locationTimezone,
        LocationAddress address
        /*,IEnumerable<DepartmentLocations> departments*/)
    {
        var newLocationId = LocationId.Create();
        return new Location(newLocationId, locationName, address, locationTimezone/*, departments*/);
    }
}