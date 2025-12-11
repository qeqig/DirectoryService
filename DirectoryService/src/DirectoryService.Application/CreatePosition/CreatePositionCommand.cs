using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Position;

namespace DirectoryService.Application.CreatePosition;

public record CreatePositionCommand(CreatePositionDto CreatePositionDto) : ICommand;