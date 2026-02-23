namespace Analytics.Application.DTOs;
public class PeriodAverageDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
    public double Precipitation { get; set; }
}
