namespace Analitycs.Domain.Entity;
public class SensorData
{
    public string PlotId { get; set; } = string.Empty;
    public List<HourlyAverage> HourlyAverages { get; set; } = new();
    public PeriodAverage? PeriodAverage { get; set; }
}
