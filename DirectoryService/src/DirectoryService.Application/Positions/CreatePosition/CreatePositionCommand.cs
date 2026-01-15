using Core.Abstractions;
using DirectoryService.Contracts.Position;

namespace DirectoryService.Application.Positions.CreatePosition;

public record CreatePositionCommand(CreatePositionDto CreatePositionDto) : ICommand;