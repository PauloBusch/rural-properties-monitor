namespace Analitycs.Domain.Entity.Property;
public class Property
{
    public string Id { get; set; } = string.Empty;
    public string ProducerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<Plot> Plots { get; set; } = new();
}
