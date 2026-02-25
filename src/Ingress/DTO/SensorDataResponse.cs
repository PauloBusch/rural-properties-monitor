namespace IngressApi.DTO;

public class SensorDataResponse
{
    public string PlotId { get; set; } = string.Empty;
    public List<HourlyAverage> HourlyAverages { get; set; } = new();
    public PeriodAverage PeriodAverage { get; set; } = new();
}

public class HourlyAverage
{
    public DateTime Hour { get; set; }
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
    public double Precipitation { get; set; }
}

public class PeriodAverage
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
    public double Precipitation { get; set; }
}

