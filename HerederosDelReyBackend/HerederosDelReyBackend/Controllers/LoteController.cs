using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerederosDelReyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoteController : ControllerBase
    {
        private readonly ILoteService _service;
        public LoteController(ILoteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var lote = await _service.GetByIdAsync(id);

            if (lote == null)
                return NotFound(new { mensaje = "lote no encontrada" });

            return Ok(lote);
        }

        [HttpPost]
        public async Task<IActionResult> Create(LoteCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creado = await _service.AddAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, LoteUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _service.UpdateAsync(id, dto);

            if (!ok)
                return NotFound(new { mensaje = "Lote no encontrada" });

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);

            if (!ok)
                return NotFound(new { mensaje = "Lote no encontrada" });

            return NoContent();
        }

        [HttpGet("Paginacion")]
        public async Task<IActionResult> GetAllPag([FromQuery] PostQueryFilter filter)
        {
            var response = await _service.GetAllAsync(filter);
            return Ok(response);
        }

        // GET api/Lote/vencimientos?idSucursal=1&dias=90
        // Lotes con stock vencidos o que vencen dentro de "dias" (por defecto 90).
        [HttpGet("vencimientos")]
        public async Task<IActionResult> GetVencimientos(
            [FromQuery] int? idSucursal,
            [FromQuery] int dias = 90)
        {
            if (dias < 1 || dias > 365)
                return BadRequest(new { mensaje = "El rango de días debe estar entre 1 y 365." });

            var lotes = await _service.GetVencimientosAsync(idSucursal, dias);

            return Ok(lotes);
        }

        // POST api/Lote/5/retirar  { "cantidad": 8 }
        // Da de baja unidades del lote y del stock de su sucursal.
        [Authorize]
        [HttpPost("{id:int}/retirar")]
        public async Task<IActionResult> Retirar(int id, [FromBody] RetirarLoteDto dto)
        {
            try
            {
                await _service.RetirarAsync(id, dto.Cantidad);

                return Ok(new { mensaje = "Unidades retiradas del inventario." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
