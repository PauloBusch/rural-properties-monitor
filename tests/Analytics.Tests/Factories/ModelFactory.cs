using Analitycs.Domain.Entity;
using Analitycs.Domain.Entity.Property;

namespace Analytics.Tests.Factories;

public class ModelFactory
{
    private readonly DateTime _startDate;
    private readonly DateTime _endDate;

    public ModelFactory()
    {
        _endDate = DateTime.UtcNow;
        _startDate = _endDate.AddDays(-1);
    }

    public List<SensorData> SensorDataList => new()
    {
        new SensorData
        {
            PlotId = "plot-1",
            HourlyAverages = new List<HourlyAverage>
            {
                new HourlyAverage
                {
                    Hour = _endDate.AddHours(-2),
                    SoilMoisture = 35.4,
                    Temperature = 24.5,
                    Precipitation = 0
                },
                new HourlyAverage
                {
                    Hour = _endDate.AddHours(-1),
                    SoilMoisture = 36.1,
                    Temperature = 25.2,
                    Precipitation = 1.2
                }
            },
            PeriodAverage = new PeriodAverage
            {
                StartDate = _startDate,
                EndDate = _endDate,
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

    public DateTime StartDate => _startDate;
    public DateTime EndDate => _endDate;

    public string Token => "fake-token";

    public List<Property> Properties => new()
    {
        new Property
        {
            Id = "prop-1",
            ProducerId = "producer-1",
            Name = "Farm 1",
            Location = "MT",
            Plots = new List<Plot>
            {
                new Plot
                {
                    PlotId = "plot-1",
                    Name = "Plot A",
                    CropType = "Soy",
                    AreaHectares = 10
                }
            }
        }
    };

    public string ProducerId => "producer-1";
}