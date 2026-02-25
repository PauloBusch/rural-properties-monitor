using System.ComponentModel.DataAnnotations;

namespace Properties.DTO;

public class PlotCreateDto
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string CropType { get; set; } = null!;

    [Range(0.1, 10000)]
    public double AreaHectares { get; set; }
}