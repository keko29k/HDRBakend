namespace HerederosDelReyBackend.DTOs.DTO
{
    // Lote con stock disponible que ya venció o vence dentro del rango consultado.
    public class LoteVencimientoDto
    {
        public int IdLote { get; set; }
        public string NumeroLote { get; set; } = null!;

        public int IdProducto { get; set; }
        public string Codigo { get; set; } = null!;
        public string Producto { get; set; } = null!;

        public int IdSucursal { get; set; }
        public string Sucursal { get; set; } = null!;

        public DateOnly? FechaFabricacion { get; set; }
        public DateOnly FechaVencimiento { get; set; }

        // Negativo cuando el lote ya venció.
        public int DiasRestantes { get; set; }

        public decimal CantidadInicial { get; set; }
        public decimal CantidadDisponible { get; set; }

        public decimal CostoCompra { get; set; }
        public decimal PrecioVenta { get; set; }

        // CantidadDisponible × costo / precio de venta.
        public decimal ValorCosto { get; set; }
        public decimal ValorVenta { get; set; }
    }
}
