using Core.Abstractions;
using DirectoryService.Contracts.Location;

namespace DirectoryService.Application.Locations.CreateLocation;

public record CreateLocationCommand(CreateLocationDto CreateLocationDto) : ICommand;