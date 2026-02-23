using Microsoft.AspNetCore.Mvc;
using Properties.Models;
using Properties.Services;

namespace Properties.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly PropertyService _propertyService;

    public PropertiesController(PropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    /// <summary>
    /// Cadastra uma nova propriedade para o produtor logado.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProperty(Property property)
    {
        if (string.IsNullOrEmpty(property.ProducerId))
            return BadRequest("O ID do produtor é obrigatório.");

        await _propertyService.CreateAsync(property);
        return CreatedAtAction(nameof(GetByProducer), new { producerId = property.ProducerId }, property);
    }

    /// <summary>
    /// Consulta todas as propriedades e seus respectivos talhões de um produtor.
    /// </summary>
    [HttpGet("producer/{producerId}")]
    public async Task<ActionResult<List<Property>>> GetByProducer(string producerId)
    {
        var properties = await _propertyService.GetAsync(producerId);
        return Ok(properties);
    }

    /// <summary>
    /// Cadastra um novo talhão dentro de uma propriedade existente.
    /// </summary>
    [HttpPost("{propertyId}/plots")]
    public async Task<IActionResult> AddPlot(string propertyId, Plot plot)
    {
        var property = await _propertyService.GetByIdAsync(propertyId);
        if (property == null)
            return NotFound("Propriedade não encontrada.");

        await _propertyService.AddPlotAsync(propertyId, plot);
        return NoContent();
    }
}