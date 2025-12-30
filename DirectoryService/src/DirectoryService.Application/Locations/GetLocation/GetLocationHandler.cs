using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Location;
using DirectoryService.Domain.Department.VO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.GetLocation;

public class GetLocationHandler : IQueryHandler<GetLocationResponse, GetLocationQuery>
{
    private readonly IReadDbContext _readDbContext;

    private readonly ILogger _logger;

    public GetLocationHandler(IReadDbContext readDbContext, ILogger<GetLocationHandler> logger)
    {
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<GetLocationResponse>> Handle(GetLocationQuery query, CancellationToken cancellationToken = default)
    {
        var locationQuery = _readDbContext.LocationsRead;

        if (!string.IsNullOrWhiteSpace(query.Dto.Search))
            locationQuery = locationQuery.Where(l => l.LocationName.Value.ToLower().Contains(query.Dto.Search.ToLower()));

        if (query.Dto.IsActive.HasValue)
            locationQuery = locationQuery.Where(l => l.IsActive == query.Dto.IsActive.Value);

        if (query.Dto.DepartmentIds != null && query.Dto.DepartmentIds.Length > 0)
        {
            var departIds = query.Dto.DepartmentIds.Select(DepartmentId.Current).ToList();
            locationQuery = locationQuery.Where(l => l.Departments.Any(d => departIds.Contains(d.DepartmentId)));
        }

        long totalCount = await locationQuery.LongCountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.Dto.SortBy))
        {
            switch (query.Dto.SortBy)
            {
                case "Name":
                    locationQuery = locationQuery.OrderBy(l => l.LocationName.Value);
                    break;
                case "Created":
                    locationQuery = locationQuery.OrderBy(l => l.CreatedAt);
                    break;
            }
        }

        int pageSkip = 0;

        if (query.Dto.Page != null && query.Dto.Page > 0)
            pageSkip = query.Dto.Page.Value - 1;

        locationQuery = locationQuery
            .Skip(pageSkip * (query.Dto.PageSize ?? 1))
            .Take(query.Dto.PageSize ?? 20);

        var locations = await locationQuery.Select(l => new LocationDto
        {
            Id = l.Id.Value,
            Name = l.LocationName.Value,
            Country = l.Address.Country,
            City = l.Address.City,
            Street = l.Address.Street,
            HouseNumber = l.Address.HouseNumber,
            Timezone = l.LocationTimezone.Value,
            IsActive = l.IsActive,
            Created = l.CreatedAt,
            Updated = l.UpdatedAt,
        }).ToListAsync(cancellationToken);

        return new GetLocationResponse(locations, totalCount);

    }
}