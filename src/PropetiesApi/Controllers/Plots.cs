using Microsoft.AspNetCore.Mvc;
using PropertiesService.Application.DTOs;
using PropertiesService.Domain;
using PropetiesApi.Application.Interfaces;

namespace PropetiesApi.Controllers
{
    [ApiController]
    [Route("api/properties/{propertyId}/plots")]
    public class Plots : ControllerBase
    {
        private readonly IPlotService _service;

        public Plots(IPlotService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obter todos os plots de uma propriedade
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Plot>>> GetAll(string propertyId)
        {
            var plots = await _service.GetAllByPropertyIdAsync(propertyId);
            return Ok(plots);
        }

        /// <summary>
        /// Obter plot por ID
        /// </summary>
        [HttpGet("{plotId}")]
        public async Task<ActionResult<Plot>> GetById(string propertyId, string plotId)
        {
            var plot = await _service.GetByIdAsync(propertyId, plotId);
            
            if (plot == null)
            {
                return NotFound(new { message = $"Plot com ID {plotId} não encontrado na propriedade {propertyId}" });
            }

            return Ok(plot);
        }

        /// <summary>
        /// Criar novo plot em uma propriedade
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Plot>> Create(string propertyId, [FromBody] CreatePlotRequest request)
        {
            var plot = await _service.CreateAsync(propertyId, request);

            if (plot == null)
            {
                return NotFound(new { message = $"Propriedade com ID {propertyId} não encontrada" });
            }

            return CreatedAtAction(nameof(GetById), new { propertyId, plotId = plot.Id }, plot);
        }

        /// <summary>
        /// Atualizar plot existente
        /// </summary>
        [HttpPut("{plotId}")]
        public async Task<ActionResult<Plot>> Update(string propertyId, string plotId, [FromBody] UpdatePlotRequest request)
        {
            var plot = await _service.UpdateAsync(propertyId, plotId, request);
            
            if (plot == null)
            {
                return NotFound(new { message = $"Plot com ID {plotId} não encontrado na propriedade {propertyId}" });
            }

            return Ok(plot);
        }

        /// <summary>
        /// Deletar plot
        /// </summary>
        [HttpDelete("{plotId}")]
        public async Task<ActionResult> Delete(string propertyId, string plotId)
        {
            var deleted = await _service.DeleteAsync(propertyId, plotId);
            
            if (!deleted)
            {
                return NotFound(new { message = $"Plot com ID {plotId} não encontrado na propriedade {propertyId}" });
            }

            return NoContent();
        }
    }
}
