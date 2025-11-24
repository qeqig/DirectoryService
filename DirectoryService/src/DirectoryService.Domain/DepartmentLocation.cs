using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public class DepartmentLocation
{
    // EF core
    private DepartmentLocation() { }

    private DepartmentLocation(Guid departmentId, Guid locationId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    public Guid Id { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid LocationId { get; private set; }

    public static Result<DepartmentLocation> Create(Guid departmentId, Guid locationId)
    {
        if (departmentId == Guid.Empty || locationId == Guid.Empty)
            return Result.Failure<DepartmentLocation>("Department Id and Location Id cannot be empty");

        return new DepartmentLocation(departmentId, locationId);
    }
}