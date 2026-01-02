using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Department.GetTopDepartmentsByPosition;
using Microsoft.EntityFrameworkCore;


namespace DirectoryService.Application.Departments.GetTopDepartmentsByPosition;

public class GetTopDepartmentsByPositionHandler : IQueryHandler<GetTopDepartmentResponse, GetTopDepartmentsByPositionQuery>
{
    private readonly IReadDbContext _readDbContext;

    public GetTopDepartmentsByPositionHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<GetTopDepartmentResponse>> Handle(GetTopDepartmentsByPositionQuery query, CancellationToken cancellationToken = default)
    {
        var departments = await _readDbContext.DepartmentsRead
            .Select(d => new TopDepartmentDto
            {
                Id = d.Id.Value,
                Name = d.DepartmentName.Value,
                Path = d.Path.Value,
                Depth = d.Depth,
                CreatedAt = d.CreatedAt,
                PositionCount = d.Positions.Count,
                IsActive = d.IsActive,
            })
            .OrderByDescending(d => d.PositionCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new GetTopDepartmentResponse(departments);
    }
}