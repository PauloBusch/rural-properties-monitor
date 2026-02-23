namespace Analitycs.Domain.Entity;
public class HourlyAverage
{
    public DateTime Hour { get; set; }
    public double SoilMoisture { get; set; }
    public double Temperature { get; set; }
    public double Precipitation { get; set; }
}
