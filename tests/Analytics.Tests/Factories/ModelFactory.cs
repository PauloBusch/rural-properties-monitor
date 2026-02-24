using Analitycs.Domain.Entity;

namespace Analytics.Tests.Factories;

public class ModelFactory
{
    public List<SensorData> SensorDataList => new()
    {
        new SensorData
        {
            PlotId = "plot-1",
            HourlyAverages = new List<HourlyAverage>
            {
                new HourlyAverage
                {
                    Hour = DateTime.UtcNow.AddHours(-2),
                    SoilMoisture = 35.4,
                    Temperature = 24.5,
                    Precipitation = 0
                },
                new HourlyAverage
                {
                    Hour = DateTime.UtcNow.AddHours(-1),
                    SoilMoisture = 36.1,
                    Temperature = 25.2,
                    Precipitation = 1.2
                }
            },
            PeriodAverage = new PeriodAverage
            {
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow,
                SoilMoisture = 35.75,
                Temperature = 24.85,
                Precipitation = 0.6
            }
        }
    };

    public List<string> PlotIds => new()
    {
        "plot-1",
        "plot-2"
    };

    public DateTime StartDate => DateTime.UtcNow.AddDays(-1);

    public DateTime EndDate => DateTime.UtcNow;

    public string Token => "fake-token";
}