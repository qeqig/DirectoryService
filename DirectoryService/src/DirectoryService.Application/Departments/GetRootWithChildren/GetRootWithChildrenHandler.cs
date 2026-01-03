using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Department.GetRootWithChildren;

namespace DirectoryService.Application.Departments.GetRootWithChildren;

public class GetRootWithChildrenHandler : IQueryHandler<GetRootWithChildrenResponse, GetRootWithChildrenQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetRootWithChildrenHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<GetRootWithChildrenResponse>> Handle(GetRootWithChildrenQuery query, CancellationToken cancellationToken = default)
    {
        var sql = """
                  WITH roots AS (
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
                  WHERE d.parent_id IS NULL
                  ORDER BY d.created_at
                  OFFSET @root_skip
                  LIMIT @size)
                  
                  SELECT *, 
                        (EXISTS(SELECT 1 FROM departments d WHERE d.parent_id = roots.id OFFSET @prefetch LIMIT 1)) AS has_more_children
                  FROM roots
                  
                  UNION ALL
                  
                  SELECT c.*, 
                        (EXISTS(SELECT 1 FROM departments d WHERE d.parent_id = c.id)) AS has_more_children 
                  FROM roots r 
                          CROSS JOIN LATERAL(SELECT d.id,
                                d.name,
                                d.identifier,
                                d.parent_id,
                                d.path,
                                d.depth,
                                d.is_active,
                                d.created_at,
                                d.updated_at
                  FROM departments d
                  WHERE d.parent_id = r.id AND d.is_active = true
                  ORDER BY d.created_at
                  LIMIT @prefetch) c
                  """;

        var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        int rootSkip = (query.Dto.Page - 1 ?? 0) * (query.Dto.Size ?? 20);

        var departmentsWithChild = (await connection.QueryAsync<DepartmentDto>(sql, param: new
        {
            size = query.Dto.Size ?? 20,
            root_skip = rootSkip,
            prefetch = query.Dto.Prefetch ?? 3,
        })).ToList();

        int rootCount = departmentsWithChild.Count(d => d.ParentId == null);

        return new GetRootWithChildrenResponse(departmentsWithChild, rootCount);
    }
}