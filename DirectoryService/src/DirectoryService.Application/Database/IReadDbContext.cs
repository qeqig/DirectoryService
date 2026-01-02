using DirectoryService.Domain.Department;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Position;

namespace DirectoryService.Application.Database;

public interface IReadDbContext
{
    IQueryable<Location> LocationsRead { get; }

    IQueryable<Position> PositionsRead { get; }

    IQueryable<Department> DepartmentsRead { get; }
}