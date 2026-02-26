namespace Analitycs.Domain.Entity.Property;
public class Plot
{
    public string PlotId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CropType { get; set; } = string.Empty;
    public double AreaHectares { get; set; }
}
