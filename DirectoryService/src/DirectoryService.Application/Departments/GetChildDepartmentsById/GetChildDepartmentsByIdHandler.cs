using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Department.GetChildDepartmentsById;
using DirectoryService.Contracts.Department.GetRootWithChildren;
using DirectoryService.Domain;
using Microsoft.Extensions.Caching.Hybrid;
using Shared;

namespace DirectoryService.Application.Departments.GetChildDepartmentsById;

public class GetChildDepartmentsByIdHandler : IQueryHandler<GetChildDepartmentsByIdResponse, GetChildDepartmentsByIdQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    private readonly HybridCache _cache;

    public GetChildDepartmentsByIdHandler(IDbConnectionFactory dbConnectionFactory, HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cache = cache;
    }

    public async Task<Result<GetChildDepartmentsByIdResponse>> Handle(GetChildDepartmentsByIdQuery query, CancellationToken cancellationToken = default)
    {
        var prefix = $"{Constants.DEPARTMENT_CACHE_KEY}_child";

        var key = KeyBuilder.Build(
            prefix,
            ("parent_id", query.ParentId),
            ("page", query.Dto.Page),
            ("size", query.Dto.Size));

        var options = new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(Constants.TTL_CACHE) };

        var departments = await _cache.GetOrCreateAsync(
            key,
            async _ => await GetChildDepartmentsByIdAsync(query,  cancellationToken),
            options,
            cancellationToken: cancellationToken);

        return departments;
    }

    private async Task<GetChildDepartmentsByIdResponse> GetChildDepartmentsByIdAsync(
            GetChildDepartmentsByIdQuery query, CancellationToken cancellationToken = default)
        {
            var sql = """
                      WITH child AS (
                            SELECT d.id,
                                    d.name,
                                    d.identifier,
                                    d.parent_id,
                                    d.path,
                                    d.depth,
                                    d.is_active,
                                    d.created_at,
                                    d.updated_at
                      FROM departments d
                      WHERE d.parent_id = @parentId
                      ORDER BY d.created_at
                      OFFSET @childOffset
                      LIMIT @size)

                      SELECT *, 
                            (EXISTS(SELECT 1 FROM departments d WHERE d.parent_id = child.id LIMIT 1)) AS has_more_children
                      FROM child
                      """;

            var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var childDepartmentsById = (await connection.QueryAsync<DepartmentDto>(
                sql,
                param: new
                {
                    parentId = query.ParentId,
                    size = query.Dto.Size,
                    childOffset = (query.Dto.Page - 1 ?? 0) * (query.Dto.Size ?? 20),
                })).ToList();

            return new GetChildDepartmentsByIdResponse(childDepartmentsById);
        }
    }