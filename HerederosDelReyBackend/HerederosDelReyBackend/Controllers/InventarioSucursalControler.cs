using HerederosDelReyBackend.DTOs;
using HerederosDelReyBackend.DTOs.DTO_CREATE;
using HerederosDelReyBackend.DTOs.DTO_UPDATE;
using HerederosDelReyBackend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HerederosDelReyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioSucursalControler : ControllerBase
    {
        private readonly IInventarioSucursalService _service;
        public InventarioSucursalControler(IInventarioSucursalService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sucursalDto = await _service.GetByIdAsync(id);

            if (sucursalDto == null)
                return NotFound(new { mensaje = "sucursal no encontrada" });

            return Ok(sucursalDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(InventarioSucursalCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var creado = await _service.AddAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, InventarioSucursalUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ok = await _service.UpdateAsync(id, dto);

            if (!ok)
                return NotFound(new { mensaje = "inventario no encontrado" });

            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);

            if (!ok)
                return NotFound(new { mensaje = "producto no encontrado" });

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

