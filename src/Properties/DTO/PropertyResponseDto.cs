namespace Properties.DTO;

public class PropertyResponseDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string ProducerId { get; set; } = null!;
    public List<PlotResponseDto> Plots { get; set; } = new();

    public double TotalAreaHectares => Plots.Sum(p => p.AreaHectares);
}