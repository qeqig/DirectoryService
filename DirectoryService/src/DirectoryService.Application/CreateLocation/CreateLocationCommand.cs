using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Location;

namespace DirectoryService.Application.CreateLocation;

public record CreateLocationCommand(CreateLocationDTO CreateLocationDto) : ICommand;