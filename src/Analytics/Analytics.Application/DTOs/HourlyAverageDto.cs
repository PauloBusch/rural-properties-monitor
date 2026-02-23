namespace Analytics.Application.DTOs;
public class HourlyAverageDto
{
    public DateTime Hour { get; set; }
    public double SoilMoisture { get; set; }
    public DateTime Temperature { get; set; }
    public DateTime Precipitation { get; set; }
}
