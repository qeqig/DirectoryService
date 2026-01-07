using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class DeleteDepartmentService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<DeleteDepartmentService> _logger;

    public DeleteDepartmentService(IServiceScopeFactory scopeFactory,  ILogger<DeleteDepartmentService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
            _logger.LogInformation("Starting delete department service");

            using var scope = _scopeFactory.CreateAsyncScope();

            var configuration = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

            var timer = new PeriodicTimer(TimeSpan.FromDays(1));

            var connection = configuration.Database.GetDbConnection();

            var sql = $"""
                       WITH delete_departments AS (DELETE FROM departments
                           WHERE is_active = false AND deleted_at <= @currentTime
                           RETURNING id, parent_id, path, depth),
                       delete_join_position AS(DELETE FROM departments_positions
                           USING delete_departments
                           WHERE department_id = delete_departments.id),
                       delete_join_location AS (DELETE FROM departments_locations
                           USING delete_departments
                           WHERE department_id = delete_departments.id),
                       update_child AS (UPDATE departments depart SET 
                               parent_id = delete_departments.parent_id,
                               path = (subpath(delete_departments.path, 0, -1) || depart.name::ltree),
                               depth = depart.depth - 1
                           FROM delete_departments
                           WHERE depart.parent_id = delete_departments.id
                           RETURNING depart.id, depart.path)
                       UPDATE departments dep SET
                               path = (subpath(update_child.path, 0, -1) || subpath(dep.path, delete_departments.depth)),
                               depth = dep.depth - 1
                           FROM update_child, delete_departments
                           WHERE dep.path <@ delete_departments.path AND dep.id != delete_departments.id AND dep.id != update_child.id
                       """;

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var result = await connection.ExecuteAsync(sql, param: new { currentTime = DateTime.Now });
                if (result > 0)
                    _logger.LogInformation("Deleted department");
            }
    }
}