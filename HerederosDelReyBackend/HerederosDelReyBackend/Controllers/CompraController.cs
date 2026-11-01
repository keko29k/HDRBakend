using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HerederosDelReyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompraController : ControllerBase
    {
        private readonly ICompraService _service;
        public CompraController(ICompraService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var compra = await _service.GetByIdAsync(id);

            if (compra == null)
                return NotFound(new { mensaje = "compra no encontrada" });

            return Ok(compra);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CompraCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creado = await _service.AddAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CompraUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _service.UpdateAsync(id, dto);

            if (!ok)
                return NotFound(new { mensaje = "compra no encontrada" });

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);

            if (!ok)
                return NotFound(new { mensaje = "compra no encontrada" });

            return NoContent();
        }
        [HttpGet("Paginacion")]
        public async Task<IActionResult> GetAllPag([FromQuery] PostQueryFilter filter)
        {
            var response = await _service.GetAllAsync(filter);
            return Ok(response);
        }


        [HttpPost("Registrar")]
        [Authorize]
        public async Task<IActionResult> Registrar([FromBody] RegistrarCompraDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Obtener usuario desde el token
            var usuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Obtener sucursal desde el token
            var sucursalClaim = User.FindFirstValue("IdSucursal");

            // Obtener rol desde el token
            var rol = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(usuarioClaim, out int idUsuario))
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario."
                });

            if (!int.TryParse(sucursalClaim, out int idSucursal))
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar la sucursal del usuario."
                });

            try
            {
                var compra = await _service.RegistrarAsync(
                    dto,
                    idUsuario,
                    idSucursal,
                    rol
                );

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = compra.Id },
                    compra
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
