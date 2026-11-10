using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.Exceptions;
using HerederosDelReyBackend.Interfaces;
using HerederosDelReyBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerederosDelReyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }
       // [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _usuarioService.GetAllAsync();
            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await _usuarioService.GetByIdAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(usuario);
        }

        [HttpPost]
      //  [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] UsuarioCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (creado, conflicto) = await Duplicados.IntentarAsync(() => _usuarioService.AddAsync(dto));
            if (conflicto != null) return conflicto;
            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (actualizado, conflicto) = await Duplicados.IntentarAsync(() => _usuarioService.UpdateAsync(id, dto));
            if (conflicto != null) return conflicto;
            if (!actualizado)
                return BadRequest(new { mensaje = "No se pudo actualizar el usuario" });

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _usuarioService.DeleteAsync(id);
            if (!eliminado)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return NoContent();
        }

        [HttpGet("pagination")]
        public async Task<IActionResult> GetAllPagination([FromQuery] PostQueryFilter filter)
        {
            var response = await _usuarioService.GetAllAsync(filter);
            return Ok(response);
        }

    }

}
