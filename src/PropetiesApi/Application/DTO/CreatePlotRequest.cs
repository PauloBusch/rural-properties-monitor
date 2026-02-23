namespace PropertiesService.Application.DTOs;

public record CreatePlotRequest(
    string Name,
    string Crop,
    double AreaHectares,
    double Latitude,
    double Longitude
);