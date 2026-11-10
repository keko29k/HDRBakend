using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Exceptions;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerederosDelReyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaService _service;
        public CategoriaController(ICategoriaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var categoria = await _service.GetByIdAsync(id);

            if (categoria == null)
                return NotFound(new { mensaje = "Categoria no encontrada" });

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoriaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (creado, conflicto) = await Duplicados.IntentarAsync(() => _service.AddAsync(dto));
            if (conflicto != null) return conflicto;

            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CategoriaUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (ok, conflicto) = await Duplicados.IntentarAsync(() => _service.UpdateAsync(id, dto));
            if (conflicto != null) return conflicto;

            if (!ok)
                return NotFound(new { mensaje = "Categoria no encontrada" });

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);

            if (!ok)
                return NotFound(new { mensaje = "Categoria no encontrada" });

            return NoContent();
        }

        [HttpGet("Paginacion")]
        public async Task<IActionResult> GetAllPag([FromQuery] PostQueryFilter filter)
        {
            var response = await _service.GetAllAsync(filter);
            return Ok(response);
        }
    }
}
