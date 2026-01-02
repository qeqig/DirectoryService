namespace DirectoryService.Contracts.Location;

public record GetLocationResponse(List<LocationDto> Locations, long TotalCount);