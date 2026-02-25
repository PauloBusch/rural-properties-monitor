using System.ComponentModel.DataAnnotations;

namespace Properties.DTO;

public class PropertyCreateDto
{
    [Required(ErrorMessage = "The property name is required.")]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Location/region is required.")]
    public string Location { get; set; } = null!;

    [Required]
    public string ProducerId { get; set; } = null!;
}