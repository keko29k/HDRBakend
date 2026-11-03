namespace HerederosDelReyBackend.DTOs.DTO
{
    // =====================================================
    // Vista completa de una venta: cabecera, productos y pagos.
    // Solo lectura (GET api/Venta/{id}/detalle).
    // =====================================================
    public class VentaVistaDto
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; }
        public string EstadoVenta { get; set; } = null!;

        public bool Acredito { get; set; }
        public decimal Total { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public DateOnly? FechaLimitePago { get; set; }
        public string? Observacion { get; set; }

        public string Cliente { get; set; } = null!;
        public string? ClienteDocumento { get; set; }
        public string Vendedor { get; set; } = null!;
        public string Sucursal { get; set; } = null!;

        public List<VentaVistaItemDto> Items { get; set; } = new();
        public List<VentaVistaPagoDto> Pagos { get; set; } = new();
    }

    public class VentaVistaItemDto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; } = null!;
        public string Producto { get; set; } = null!;
        public string? NumeroLote { get; set; }
        public DateOnly? FechaVencimiento { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Descuento { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class VentaVistaPagoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string MetodoPago { get; set; } = null!;
        public decimal Monto { get; set; }
        public int IdCaja { get; set; }
        public string? Referencia { get; set; }
        public string? Observacion { get; set; }
    }

    // =====================================================
    // Vista completa de una compra: cabecera y productos.
    // Solo lectura (GET api/Compra/{id}/detalle).
    // =====================================================
    public class CompraVistaDto
    {
        public int Id { get; set; }
        public DateTime FechaCompra { get; set; }
        public string EstadoCompra { get; set; } = null!;

        public string TipoDocumento { get; set; } = null!;
        public string? NumeroDocumento { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string? Observacion { get; set; }

        public string Proveedor { get; set; } = null!;
        public string? ProveedorNit { get; set; }
        public string Sucursal { get; set; } = null!;
        public string Usuario { get; set; } = null!;

        public List<CompraVistaItemDto> Items { get; set; } = new();
    }

    public class CompraVistaItemDto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; } = null!;
        public string Producto { get; set; } = null!;
        public string? NumeroLote { get; set; }
        public DateOnly? FechaVencimiento { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal Descuento { get; set; }
        public decimal Impuesto { get; set; }
        public decimal SubTotal { get; set; }
    }
}
