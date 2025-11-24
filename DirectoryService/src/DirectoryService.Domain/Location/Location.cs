using CSharpFunctionalExtensions;
using DirectoryService.Domain.Location.VO;

namespace DirectoryService.Domain.Location;

public class Location
{
    // EF core
    private Location() { }

    private List<DepartmentLocation> _departments = [];

    private Location(
        Guid id,
        LocationName locationName,
        LocationAddress address,
        LocationTimezone locationTimezone,
        IEnumerable<DepartmentLocation> departments)
    {
        Id = id;
        LocationName = locationName;
        Address = address;
        LocationTimezone = locationTimezone;
        _departments = departments.ToList();
    }

    public Guid Id { get; private set; }

    public LocationName LocationName { get; private set; }

    public LocationAddress Address { get; private set; }

    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public LocationTimezone LocationTimezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Location> Create(
        Guid id,
        LocationName locationName,
        LocationTimezone locationTimezone,
        LocationAddress address,
        IEnumerable<DepartmentLocation> departments)
    {
        return new Location(id, locationName, address, locationTimezone, departments);
    }
}