using Analitycs.Application.Interfaces;
using Analitycs.Domain.Entity;
using Analitycs.Domain.Interfaces;
using Analytics.Application.DTOs;

public class DashboardService : IDashboardService
{
    private readonly IPropertiesApiClient _propertiesApiClient;
    private readonly IIngressApiClient _ingressApiClient;

    public DashboardService(
        IPropertiesApiClient propertiesApiClient,
        IIngressApiClient ingressApiClient)
    {
        _propertiesApiClient = propertiesApiClient;
        _ingressApiClient = ingressApiClient;
    }

    public async Task<ProducerDashboardResponse> GetDashboardAsync(
        string producerId,
        string token,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var propertiesTask = _propertiesApiClient
        .GetPropertiesByProducerAsync(producerId, token, cancellationToken);

        var properties = await propertiesTask;

        var allPlotIds = properties
            .SelectMany(p => p.Plots)
            .Select(p => p.IdPlot)
            .ToList();

        var sensorData = await _ingressApiClient
            .GetSensorDataAsync(allPlotIds, startDate, endDate, cancellationToken)
            ?? new List<SensorData>();

        var sensorDictionary = sensorData
            .Where(s => s != null && !string.IsNullOrWhiteSpace(s.PlotId))
            .ToDictionary(s => s.PlotId.Trim().ToLower(), s => s);

        var response = new ProducerDashboardResponse
        {
            ProducerId = producerId,
            Properties = properties.Select(property => new PropertyDashboardDto
            {
                PropertyId = property.Id,
                Name = property.Name,
                Location = property.Location,

                Plots = property.Plots.Select(plot =>
                {
                    sensorDictionary.TryGetValue(plot.IdPlot, out var sensor);

                    return new PlotDashboardDto
                    {
                        PlotId = plot.IdPlot,
                        PlotName = plot.Name,
                        CropType = plot.CropType,
                        AreaHectares = plot.AreaHectares,

                        PeriodAverage = sensor?.PeriodAverage == null
                            ? null
                            : new PeriodAverageDto
                            {
                                StartDate = sensor.PeriodAverage.StartDate,
                                EndDate = sensor.PeriodAverage.EndDate,
                                SoilMoisture = sensor.PeriodAverage.SoilMoisture,
                                Temperature = sensor.PeriodAverage.Temperature,
                                Precipitation = sensor.PeriodAverage.Precipitation
                            },

                                            HourlyAverages = sensor?.HourlyAverages?
                            .Select(h => new HourlyAverageDto
                            {
                                Hour = h.Hour,
                                SoilMoisture = h.SoilMoisture,
                                Temperature = h.Temperature,
                                Precipitation = h.Precipitation
                            })
                            .ToList() ?? new List<HourlyAverageDto>()
                    };
                }).ToList()

            }).ToList()
        };

        return response;
    }
}