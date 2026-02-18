using IngressApi.DTO;
using IngressApi.Repositories;
using Sensors.Models;

namespace IngressApi.Services;

public interface ISensorDataService
{
    Task<List<SensorDataResponse>> GetAggregatedDataAsync(
        List<string> plotIds,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

public class SensorDataService : ISensorDataService
{
    private readonly ISensorDataRepository _repository;

    public SensorDataService(ISensorDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SensorDataResponse>> GetAggregatedDataAsync(
        List<string> plotIds,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var rawData = await _repository.GetByPlotIdsAsync(plotIds, startDate, endDate, cancellationToken);

        // Agrupa os dados por PlotId
        var groupedByPlot = rawData.GroupBy(d => d.PlotId);

        var response = new List<SensorDataResponse>();

        foreach (var plotGroup in groupedByPlot)
        {
            var sensorResponse = new SensorDataResponse
            {
                PlotId = plotGroup.Key,
                HourlyAverages = CalculateHourlyAverages(plotGroup.ToList()),
                PeriodAverage = CalculatePeriodAverage(plotGroup.ToList(), startDate, endDate)
            };

            response.Add(sensorResponse);
        }

        return response;
    }

    // Calcula média por hora
    private List<HourlyAverage> CalculateHourlyAverages(List<SensorDataPayload> data)
    {
        return data
            .GroupBy(d => new DateTime(d.Timestamp.Year, d.Timestamp.Month, d.Timestamp.Day, d.Timestamp.Hour, 0, 0))
            .Select(g => new HourlyAverage
            {
                Hour = g.Key,
                SoilMoisture = Math.Round(g.Average(x => x.SoilMoisture), 2),
                Temperature = Math.Round(g.Average(x => x.Temperature), 2),
                Precipitation = Math.Round(g.Average(x => x.Precipitation), 2)
            })
            .OrderBy(h => h.Hour)
            .ToList();
    }

    // Calcula média do período completo
    private PeriodAverage CalculatePeriodAverage(List<SensorDataPayload> data, DateTime startDate, DateTime endDate)
    {
        if (!data.Any())
        {
            return new PeriodAverage
            {
                StartDate = startDate,
                EndDate = endDate
            };
        }

        return new PeriodAverage
        {
            StartDate = startDate,
            EndDate = endDate,
            SoilMoisture = Math.Round(data.Average(x => x.SoilMoisture), 2),
            Temperature = Math.Round(data.Average(x => x.Temperature), 2),
            Precipitation = Math.Round(data.Average(x => x.Precipitation), 2)
        };
    }
}

