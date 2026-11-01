namespace HerederosDelReyBackend.DTOs.DTO
{
    public class RegistrarDetalleCompraDto
    {
        public int IdProducto { get; set; }

        public decimal Cantidad { get; set; }

        public decimal PrecioCompra { get; set; }

        public decimal Descuento { get; set; }

        public decimal Impuesto { get; set; }

        // Información del lote
        public string? NumeroLote { get; set; }

        public DateTime? FechaFabricacion { get; set; }

        public DateTime? FechaVencimiento { get; set; }
    }
}