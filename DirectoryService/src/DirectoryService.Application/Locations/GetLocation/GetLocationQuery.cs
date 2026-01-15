using Core.Abstractions;
using DirectoryService.Contracts.Location;

namespace DirectoryService.Application.Locations.GetLocation;

public record GetLocationQuery(GetLocationDto Dto):IQuery;