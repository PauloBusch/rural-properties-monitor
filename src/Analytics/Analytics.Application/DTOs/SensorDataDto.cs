namespace Analytics.Application.DTOs;
public class SensorDataDto
{
    public string PlotId { get; set; } = string.Empty;
    public List<HourlyAverageDto> HourlyAverages { get; set; } = new();
    public PeriodAverageDto? PeriodAverage { get; set; }

}
