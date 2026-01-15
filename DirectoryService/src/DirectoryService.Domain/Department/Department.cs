using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using Shared.SharedKernel;
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
        IEnumerable<DepartmentLocation> locations,
        DepartmentId? parentId = null)
        : base(id)
    {
        DepartmentName = departmentName;
        Identifier = identifier;
        Depth = depth;
        Path = path;
        CreatedAt = DateTime.UtcNow;
        _departmentsLocations = locations.ToList();
        ParentId = parentId;

    }

    public DepartmentName DepartmentName { get; private set; } = null!;

    public Identifier Identifier { get; private set; } = null!;

    public DepartmentId? ParentId { get; private set; }

    public Path Path { get; private set; } = null!;

    public int Depth { get; private set; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyList<Department> ChildDepartments => _childDepartments;

    public IReadOnlyList<DepartmentLocation> Locations => _departmentsLocations;

    public IReadOnlyList<DepartmentPosition> Positions => _departmentsPositions;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; private set; }

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

        var path = parent.Path.CreateChild(parent, identifier);

        var parentId = parent.Id;

        return new Department(departmentId ?? DepartmentId.Create(), name, identifier, path, parent.Depth + 1, departmentLocationList, parentId);
    }

    public UnitResult<Error> UpdateLocationId(IEnumerable<DepartmentLocation> locations)
    {
        var locationList = locations.ToList();

        if (locationList.Count == 0)
            return GeneralErrors.ValueIsRequired("locationId");

        _departmentsLocations.Clear();

        _departmentsLocations.AddRange(locationList);

        return UnitResult.Success<Error>();
    }

    public void MoveDepartment(Department? department)
    {
        if (department == null)
        {
            Depth = 0;
            Path = Path.CreateParent(Identifier);
        }
        else
        {
            Depth = department.Depth + 1;
            Path = Path.CreateChild(department, Identifier);
        }

        ParentId = department?.Id;

        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DeletedAt.Value;
        Path = Path.CreateForSoftDelete(Path).Value;
    }
}