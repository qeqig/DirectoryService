namespace DirectoryService.Contracts.Location;

public record CreateLocationDTO(string Name, AddressDTO Address, string TimeZone);

