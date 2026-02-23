namespace PropertiesService.Application.DTOs;

public record UpdatePlotRequest(
    string Name,
    string Crop,
    double AreaHectares,
    double Latitude,
    double Longitude
);
