namespace DirectoryService.Contracts.Location;

public record CreateLocationDto(string Name, AddressDTO Address, string TimeZone);

