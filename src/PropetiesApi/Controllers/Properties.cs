using Microsoft.AspNetCore.Mvc;
using PropertiesService.Application.DTOs;
using PropertiesService.Domain;
using PropetiesApi.Application.Interfaces;

namespace PropetiesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Properties : ControllerBase
    {
        private readonly IPropertyService _service;

        public Properties(IPropertyService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obter todas as propriedades
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Property>>> GetAll()
        {
            var properties = await _service.GetAllAsync();
            return Ok(properties);
        }

        /// <summary>
        /// Obter propriedade por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Property>> GetById(string id)
        {
            var property = await _service.GetByIdAsync(id);

            if (property == null)
            {
                return NotFound(new { message = $"Propriedade com ID {id} não encontrada" });
            }

            return Ok(property);
        }

        /// <summary>
        /// Criar nova propriedade
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Property>> Create([FromBody] CreatePropertyRequest request)
        {
            var property = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = property.Id }, property);
        }

        /// <summary>
        /// Atualizar propriedade existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Property>> Update(string id, [FromBody] UpdatePropertyRequest request)
        {
            var property = await _service.UpdateAsync(id, request);

            if (property == null)
            {
                return NotFound(new { message = $"Propriedade com ID {id} não encontrada" });
            }

            return Ok(property);
        }

        /// <summary>
        /// Deletar propriedade
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = $"Propriedade com ID {id} não encontrada" });
            }

            return NoContent();
        }
    }
}
