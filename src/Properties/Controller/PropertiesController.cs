using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Properties.DTO;
using Properties.Models;
using Properties.Services;

namespace Properties.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    /// register a new property for the producer
    [HttpPost]
    public async Task<ActionResult<PropertyResponseDto>> Create(PropertyCreateDto dto)
    {
        var property = new Property
        {
            Name = dto.Name,
            Location = dto.Location,
            ProducerId = dto.ProducerId
        };

        await _propertyService.CreateAsync(property);

        var response = new PropertyResponseDto
        {
            Id = property.Id!,
            Name = property.Name,
            Location = property.Location,
            ProducerId = property.ProducerId
        };

        return CreatedAtAction(nameof(GetByProducer), new { producerId = response.ProducerId }, response);
    }


    /// view all properties and plots of land belonging to a producer
    [HttpGet("producer/{producerId}")]
    public async Task<ActionResult<IEnumerable<PropertyResponseDto>>> GetByProducer(string producerId)
    {
        var properties = await _propertyService.GetByProducerAsync(producerId);

        var response = properties.Select(p => new PropertyResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Plots = p.Plots.Select(plot => new PlotResponseDto
            {
                Name = plot.Name,
                AreaHectares = plot.AreaHectares
            }).ToList()
        });

        return Ok(response);
    }

    ///register a new plot within an existing property
    [HttpPost("{propertyId}/plots")]
    public async Task<IActionResult> AddPlot(string propertyId, PlotCreateDto dto)
    {
        var property = await _propertyService.GetByIdAsync(propertyId);
        if (property == null) return NotFound("Propriedade não encontrada.");

        var plot = new Plot
        {
            Name = dto.Name,
            CropType = dto.CropType,
            AreaHectares = dto.AreaHectares
        };

        await _propertyService.AddPlotAsync(propertyId, plot);
        return NoContent();
    }
}