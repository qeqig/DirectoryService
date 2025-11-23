using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.VO;
using Path = DirectoryService.Domain.Department.VO.Path;

namespace DirectoryService.Domain.Department;

public class Department
{
    // EF core
    private Department() { }

    private List<Department> _childDepartments = [];

    private List<DepartmentLocation> _departmentsLocations = [];

    private List<DepartmentPosition> _departmentsPositions = [];

    private Department(
        Guid id,
        DepartmentName departmentName,
        Identifier identifier,
        Path path,
        short depth,
        Guid? parentId,
        IEnumerable<Department> childDepartments,
        IEnumerable<DepartmentLocation> locations,
        IEnumerable<DepartmentPosition> positions)
    {
        Id = id;
        DepartmentName = departmentName;
        Identifier = identifier;
        Depth = depth;
        Path = path;
        ParentId = parentId;
        CreatedAt = DateTime.UtcNow;
        _childDepartments = childDepartments.ToList();
        _departmentsLocations = locations.ToList();
        _departmentsPositions = positions.ToList();
    }

    public Guid Id { get; private set; }

    public DepartmentName DepartmentName { get; private set; }

    public Identifier Identifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public Path Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<Department> ChildDepartments => _childDepartments;

    public IReadOnlyList<DepartmentLocation> Locations => _departmentsLocations;

    public IReadOnlyList<DepartmentPosition> Positions => _departmentsPositions;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Department> Create(
        Guid id,
        DepartmentName departmentName,
        Identifier identifier,
        Guid? parentId,
        Path path,
        short depth,
        IEnumerable<Department> childDepartments,
        IEnumerable<DepartmentLocation> locations,
        IEnumerable<DepartmentPosition> positions)
    {
        return new Department(id, departmentName, identifier, path, depth, parentId, childDepartments , locations, positions);
    }


}

