using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.Location.VO;

namespace DirectoryService.Domain;

public class DepartmentLocation
{
    // EF core
    private DepartmentLocation() { }

    private DepartmentLocation(DepartmentId departmentId, LocationId locationId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public Guid Id { get; private set; }

    public DepartmentId DepartmentId { get; private set; }

    public LocationId LocationId { get; private set; }
}