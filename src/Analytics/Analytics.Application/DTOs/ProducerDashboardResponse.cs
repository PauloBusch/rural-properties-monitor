namespace Analytics.Application.DTOs;
public class ProducerDashboardResponse
{
    public string ProducerId { get; set; } = string.Empty;
    public List<PropertyDashboardDto> Properties { get; set; } = new();
}
