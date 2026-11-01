namespace HerederosDelReyBackend.DTOs.DTO
{
    public class RegistrarCompraDto
    {
        public int? IdSucursal { get; set; }

        public int IdProveedor { get; set; }

        public string TipoDocumento { get; set; } = "RECIBO";

        public string? NumeroDocumento { get; set; }

        public DateTime FechaCompra { get; set; } = DateTime.Now;

        public string? Observacion { get; set; }

        public List<RegistrarDetalleCompraDto> Detalles { get; set; } = new();
    }
}