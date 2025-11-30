using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using Shared;
using Path = DirectoryService.Domain.Department.VO.Path;

namespace DirectoryService.Domain.Department;

public class Department : Entity<DepartmentId>
{
    // EF core
    private Department(DepartmentId id)
        : base(id) { }

    private readonly List<Department> _childDepartments = [];

    private readonly List<DepartmentLocation> _departmentsLocations = [];

    private readonly List<DepartmentPosition> _departmentsPositions = [];

    private Department(
        DepartmentId id,
        DepartmentName departmentName,
        Identifier identifier,
        Path path,
        int depth,
        IEnumerable<DepartmentLocation> locations
        )
        : base(id)
    {
        DepartmentName = departmentName;
        Identifier = identifier;
        Depth = depth;
        Path = path;
        CreatedAt = DateTime.UtcNow;
        _departmentsLocations = locations.ToList();

    }

    public DepartmentName DepartmentName { get; private set; } = null!;

    public Identifier Identifier { get; private set; } = null!;

    public DepartmentId? ParentId { get; private set; }

    public Path Path { get; private set; } = null!;

    public int Depth { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<Department> ChildDepartments => _childDepartments;

    public IReadOnlyList<DepartmentLocation> Locations => _departmentsLocations;

    public IReadOnlyList<DepartmentPosition> Positions => _departmentsPositions;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        Identifier identifier,
        IEnumerable<DepartmentLocation> departmentLocations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationList = departmentLocations.ToList();

        if (departmentLocationList.Count == 0)
            return Error.Validation("department.location", "Department locations must contain at least one location");

        var path = Path.CreateParent(identifier);
        return new Department(departmentId ?? DepartmentId.Create(), name, identifier, path, 0, departmentLocationList);
    }

    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        Identifier identifier,
        Department parent,
        IEnumerable<DepartmentLocation> departmentLocations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationList = departmentLocations.ToList();

        if (departmentLocationList.Count == 0)
            return Error.Validation("department.location", "Department locations must contain at  least one location");

        var path = parent.Path.CreateChild(identifier);

        return new Department(departmentId ?? DepartmentId.Create(), name, identifier, path, 0, departmentLocationList);
    }
}

