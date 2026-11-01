using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using HerederosDelReyBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HerederosDelReyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {
        private readonly IVentaService _service;

        public VentaController(IVentaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var venta = await _service.GetByIdAsync(id);

            if (venta == null)
                return NotFound(new { mensaje = "venta no encontrada" });

            return Ok(venta);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VentaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creado = await _service.AddAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, VentaUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _service.UpdateAsync(id, dto);

            if (!ok)
                return NotFound(new { mensaje = "venta no encontrada" });

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);

            if (!ok)
                return NotFound(new { mensaje = "venta no encontrada" });

            return NoContent();
        }

        [HttpGet("Paginacion")]
        public async Task<IActionResult> GetAllPag([FromQuery] PostQueryFilter filter)
        {
            var response = await _service.GetAllAsync(filter);
            return Ok(response);
        }


        [HttpPost]
        [Route("venta-detalle")]
        [Authorize]
        public async Task<IActionResult> VentaDetalle([FromBody] VentaDetalleDto dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos");

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario) ||
                !int.TryParse(User.FindFirst("IdSucursal")?.Value, out var idSucursal))
                return Unauthorized(new
                {
                    mensaje = "El token no contiene usuario y sucursal válidos"
                });

            try
            {
                var idVenta = await _service.VentaDetalle(dto, idUsuario, idSucursal);

                return Ok(new
                {
                    message = "Venta registrada correctamente",
                    idVenta = idVenta
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno",
                    error = ex.Message
                });
            }
        }
    }
}
