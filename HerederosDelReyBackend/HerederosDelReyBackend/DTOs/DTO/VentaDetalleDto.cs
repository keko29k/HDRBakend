using HerederosDelReyBackend.DTOs.DTO;

namespace HerederosDelReyBackend.DTOs
{
    public class VentaDetalleDto
    {

        public VentaDto Venta { get; set; }
        public List<DetalleVentaDto> Detalle { get; set; }
        public List<PagoDto> Pagos { get; set; } = new();
    }
}
