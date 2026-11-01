
using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HerederosDelReyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CajaController : ControllerBase
    {
        private readonly ICajaService _service;

        public CajaController(ICajaService service)
        {
            _service = service;
        }

        // OBTENER TODAS LAS CAJAS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        // OBTENER CAJA POR ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var caja = await _service.GetByIdAsync(id);

            if (caja == null)
                return NotFound(new { mensaje = "Caja no encontrada" });

            return Ok(caja);
        }

        // ABRIR CAJA DEL USUARIO AUTENTICADO
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CajaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!ObtenerDatosUsuario(out int idUsuario, out int idSucursal))
                return Unauthorized(new
                {
                    mensaje = "El token no contiene usuario y sucursal válidos"
                });

            try
            {
                var creada = await _service.AbrirCajaAsync(
                    dto,
                    idUsuario,
                    idSucursal
                );

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = creada.Id },
                    creada
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // OBTENER MI CAJA ABIERTA
        [HttpGet("MiCajaAbierta")]
        public async Task<IActionResult> MiCajaAbierta()
        {
            if (!ObtenerDatosUsuario(out int idUsuario, out int idSucursal))
                return Unauthorized(new
                {
                    mensaje = "El token no contiene usuario y sucursal válidos"
                });

            var caja = await _service.ObtenerMiCajaAbiertaAsync(
                idUsuario,
                idSucursal
            );

            if (caja == null)
                return NotFound(new
                {
                    mensaje = "No tienes una caja abierta en esta sucursal"
                });

            return Ok(caja);
        }

        // ACTUALIZAR CAJA
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CajaUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _service.UpdateAsync(id, dto);

            if (!ok)
                return NotFound(new { mensaje = "Caja no encontrada" });

            return NoContent();
        }

        // ELIMINAR CAJA (BAJA LÓGICA)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);

            if (!ok)
                return NotFound(new { mensaje = "Caja no encontrada" });

            return NoContent();
        }

        // LISTADO PAGINADO
        [HttpGet("Paginacion")]
        public async Task<IActionResult> GetAllPag(
            [FromQuery] PostQueryFilter filter)
        {
            var response = await _service.GetAllAsync(filter);

            return Ok(response);
        }

        // CERRAR CAJA
        [HttpPut("Cerrar/{id:int}")]
        public async Task<IActionResult> CerrarCaja(int id)
        {
            var response = await _service.CerrarCajaAsync(id);

            if (!response)
                return BadRequest(new
                {
                    mensaje = "No se pudo cerrar la caja"
                });

            return Ok(new
            {
                mensaje = "Caja cerrada correctamente"
            });
        }

        // LEER USUARIO Y SUCURSAL DEL TOKEN
        private bool ObtenerDatosUsuario(
            out int idUsuario,
            out int idSucursal)
        {
            idUsuario = 0;
            idSucursal = 0;

            var usuarioValido = int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out idUsuario
            );

            var sucursalValida = int.TryParse(
                User.FindFirst("IdSucursal")?.Value,
                out idSucursal
            );

            return usuarioValido &&
                   sucursalValida &&
                   idUsuario > 0 &&
                   idSucursal > 0;
        }
    }
}
