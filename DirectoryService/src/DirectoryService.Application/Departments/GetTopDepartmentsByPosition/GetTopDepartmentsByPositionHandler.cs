using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Department.GetTopDepartmentsByPosition;
using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Departments.GetTopDepartmentsByPosition;

public class GetTopDepartmentsByPositionHandler : IQueryHandler<GetTopDepartmentResponse, GetTopDepartmentsByPositionQuery>
{
    private readonly IReadDbContext _readDbContext;

    private readonly HybridCache _cache;

    public GetTopDepartmentsByPositionHandler(IReadDbContext readDbContext, HybridCache cache)
    {
        _readDbContext = readDbContext;
        _cache = cache;
    }

    public async Task<Result<GetTopDepartmentResponse>> Handle(GetTopDepartmentsByPositionQuery query, CancellationToken cancellationToken = default)
    {
        var key = $"{Constants.DEPARTMENT_CACHE_KEY}_top";

        var options = new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(Constants.TTL_CACHE) };

        var departments = await _cache.GetOrCreateAsync(
            key,
            async _ => await GetTopDepartmentsByPosition(query, cancellationToken),
            options,
            cancellationToken: cancellationToken);

        return departments;
    }

    private async Task<GetTopDepartmentResponse> GetTopDepartmentsByPosition(GetTopDepartmentsByPositionQuery query, CancellationToken cancellationToken = default) {
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