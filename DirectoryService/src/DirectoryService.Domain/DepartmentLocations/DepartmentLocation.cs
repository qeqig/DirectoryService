using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.Location.VO;

namespace DirectoryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation
{
    // EF core
    private DepartmentLocation() { }

    private DepartmentLocation(DepartmentLocationId id, DepartmentId departmentId, LocationId locationId)
    {
        Id = id;
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public DepartmentLocationId Id { get; private set; }

    public DepartmentId DepartmentId { get; private set; }

    public LocationId LocationId { get; private set; }

    public static DepartmentLocation Create(DepartmentLocationId id, DepartmentId departmentId, LocationId locationId)
    {
        return new DepartmentLocation(id,  departmentId, locationId);
    }
}