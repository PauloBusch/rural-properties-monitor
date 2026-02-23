namespace PropertiesService.Application.DTOs;

public record UpdatePropertyRequest(
    string Name,
    string City,
    string State,
    double TotalAreaHectares
);
