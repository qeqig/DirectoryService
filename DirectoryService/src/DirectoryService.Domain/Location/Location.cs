using CSharpFunctionalExtensions;
using DirectoryService.Domain.Location.VO;

namespace DirectoryService.Domain.Location;

public class Location : Entity<LocationId>
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
        /*IEnumerable<DepartmentLocation> departments*/)
        : base(id)
    {
        LocationName = locationName;
        Address = address;
        LocationTimezone = locationTimezone;
        //_departments = departments.ToList();
    }

    public LocationName LocationName { get; private set; }

    public LocationAddress Address { get; private set; }

    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public LocationTimezone LocationTimezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location> Create(
        LocationName locationName,
        LocationTimezone locationTimezone,
        LocationAddress address
        /*,IEnumerable<DepartmentLocation> departments*/)
    {
        var newLocationId = LocationId.Create();
        return new Location(newLocationId, locationName, address, locationTimezone/*, departments*/);
    }
}