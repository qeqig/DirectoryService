namespace DirectoryService.Contracts.Location;

public record GetLocationDto
(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    string? SortBy,
    int? Page,
    int? PageSize
);