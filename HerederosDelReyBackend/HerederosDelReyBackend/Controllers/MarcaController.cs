using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Exceptions;
using HerederosDelReyBackend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerederosDelReyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcaController : ControllerBase
    {
        private readonly IMarcaService _service;
        public MarcaController(IMarcaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var marca = await _service.GetByIdAsync(id);

            if (marca == null)
                return NotFound(new { mensaje = "marca no encontrada" });

            return Ok(marca);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MarcaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (creado, conflicto) = await Duplicados.IntentarAsync(() => _service.AddAsync(dto));
            if (conflicto != null) return conflicto;

            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, MarcaUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (ok, conflicto) = await Duplicados.IntentarAsync(() => _service.UpdateAsync(id, dto));
            if (conflicto != null) return conflicto;

            if (!ok)
                return NotFound(new { mensaje = "marca no encontrada" });

            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);

            if (!ok)
                return NotFound(new { mensaje = "marca no encontrada" });

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
