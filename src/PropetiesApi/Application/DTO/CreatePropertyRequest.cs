namespace PropertiesService.Application.DTOs;

public record CreatePropertyRequest(
    string Name,
    string City,
    string State,
    double TotalAreaHectares
);