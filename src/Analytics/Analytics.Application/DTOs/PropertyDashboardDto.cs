namespace Analytics.Application.DTOs;
public class PropertyDashboardDto
{
    public string PropertyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<PlotDashboardDto> Plots { get; set; } = new();
}
