using HerederosDelReyBackend.Models;

namespace HerederosDelReyBackend.DTOs.DTO
{
    public class CompraDto
    {
        public int Id { get; set; }

        public int IdSucursal { get; set; }
        public string? Sucursal { get; set; }

        public int IdProveedor { get; set; }
        public string? Proveedor { get; set; }

        public int IdUsuario { get; set; }
        public string? Usuario { get; set; }

        public string TipoDocumento { get; set; } = null!;
        public string? NumeroDocumento { get; set; }
        public DateTime FechaCompra { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }

        public string? Observacion { get; set; }
        public string EstadoCompra { get; set; } = null!;
    }
}
