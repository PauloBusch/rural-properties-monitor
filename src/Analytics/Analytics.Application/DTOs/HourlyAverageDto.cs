namespace Analytics.Application.DTOs;
public class HourlyAverageDto
{
    public DateTime Hour { get; set; }
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
    public double Precipitation { get; set; }
}
