namespace Analytics.Application.DTOs;
public class PlotDashboardDto
{
    public string PlotId { get; set; } = string.Empty;
    public string PlotName { get; set; } = string.Empty;
    public string CropType { get; set; } = string.Empty;
    public double AreaHectares { get; set; }

    public List<HourlyAverageDto> HourlyAverages { get; set; } = new();
    public PeriodAverageDto? PeriodAverage { get; set; }
}
