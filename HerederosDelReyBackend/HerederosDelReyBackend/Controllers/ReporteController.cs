using HerederosDelReyBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HerederosDelReyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReporteController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReporte([FromQuery] DateTime fechaInicial, [FromQuery] DateTime fechaFinal)
        {
            if (fechaInicial > fechaFinal)
            {
                return BadRequest("La fecha inicial no puede ser mayor que la fecha final.");
            }

            var reporte = await _reporteService.GetReport(fechaInicial, fechaFinal);

            return Ok(reporte);
        }

        // GET api/Reporte/general?desde=2026-11-01&hasta=2026-11-30&idSucursal=1
        [Authorize]
        [HttpGet("general")]
        public async Task<IActionResult> GetReporteGeneral(
            [FromQuery] DateOnly desde,
            [FromQuery] DateOnly hasta,
            [FromQuery] int? idSucursal)
        {
            if (desde == default || hasta == default)
                return BadRequest(new { mensaje = "Debes indicar la fecha inicial y la fecha final." });

            if (desde > hasta)
                return BadRequest(new { mensaje = "La fecha inicial no puede ser mayor que la fecha final." });

            if (hasta.DayNumber - desde.DayNumber > 366)
                return BadRequest(new { mensaje = "El rango máximo del reporte es de un año." });

            var reporte = await _reporteService.GetReporteGeneral(desde, hasta, idSucursal);

            return Ok(reporte);
        }
    }
}