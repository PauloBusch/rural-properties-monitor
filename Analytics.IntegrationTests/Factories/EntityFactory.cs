using Analitycs.Domain.Entity;

namespace Analytics.IntegrationTests.Factories;
public class EntityFactory
{
    public SensorData SensorData => new()
    {
        PlotId = Guid.NewGuid().ToString(),
        HourlyAverages = new List<HourlyAverage>
        {
            new HourlyAverage
            {
                Hour = DateTime.UtcNow,
                SoilMoisture = 23.5,
                Temperature = 18.2,
                Precipitation = 0.0
            },
            new HourlyAverage
            {
                Hour = DateTime.UtcNow.AddHours(-1),
                SoilMoisture = 22.8,
                Temperature = 17.9,
                Precipitation = 0.1
            }
        },
        PeriodAverage = new PeriodAverage
        {
            StartDate = DateTime.UtcNow.AddHours(-2),
            EndDate = DateTime.UtcNow,
            SoilMoisture = 23.15,
            Temperature = 18.05,
            Precipitation = 0.05
        }
    };
}
