namespace HerederosDelReyBackend.DTOs
{
    public class ReporteGeneralDto
    {
        public DateOnly Desde { get; set; }
        public DateOnly Hasta { get; set; }
        public int? IdSucursal { get; set; }
        public string Sucursal { get; set; } = "Todas las sucursales";

        public ReporteResumenDto Resumen { get; set; } = new();
        public List<ReporteVentaDiaDto> VentasPorDia { get; set; } = new();
        public List<ReporteMetodoPagoDto> VentasPorMetodo { get; set; } = new();
        public List<ReporteVendedorDto> VentasPorVendedor { get; set; } = new();
        public List<ReporteProductoDto> Productos { get; set; } = new();
        public List<ReporteProveedorDto> ComprasPorProveedor { get; set; } = new();
        public List<ReporteGastoConceptoDto> GastosPorConcepto { get; set; } = new();
        public ReporteCuentasPorCobrarDto CuentasPorCobrar { get; set; } = new();
        public ReporteInventarioDto Inventario { get; set; } = new();
    }

    public class ReporteResumenDto
    {
        public decimal TotalVentas { get; set; }
        public int CantidadVentas { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal UnidadesVendidas { get; set; }

        public decimal CostoVentas { get; set; }
        public decimal UtilidadBruta { get; set; }
        public decimal MargenBruto { get; set; }

        public decimal TotalGastos { get; set; }
        public decimal UtilidadNeta { get; set; }
        public decimal MargenNeto { get; set; }

        public decimal TotalCobrado { get; set; }
        public decimal VentasContado { get; set; }
        public decimal VentasCredito { get; set; }

        public decimal TotalCompras { get; set; }
        public int CantidadCompras { get; set; }
    }

    public class ReporteVentaDiaDto
    {
        public DateOnly Fecha { get; set; }
        public int CantidadVentas { get; set; }
        public decimal Total { get; set; }
        public decimal Costo { get; set; }
        public decimal Utilidad { get; set; }
    }

    public class ReporteMetodoPagoDto
    {
        public string Metodo { get; set; } = null!;
        public int CantidadPagos { get; set; }
        public decimal Monto { get; set; }
    }

    public class ReporteVendedorDto
    {
        public int IdUsuario { get; set; }
        public string Vendedor { get; set; } = null!;
        public int CantidadVentas { get; set; }
        public decimal Total { get; set; }
    }

    public class ReporteProductoDto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public decimal Cantidad { get; set; }
        public decimal Ventas { get; set; }
        public decimal Costo { get; set; }
        public decimal Utilidad { get; set; }
        public decimal Margen { get; set; }
    }

    public class ReporteProveedorDto
    {
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; } = null!;
        public int CantidadCompras { get; set; }
        public decimal Total { get; set; }
    }

    public class ReporteGastoConceptoDto
    {
        public string Concepto { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
    }

    public class ReporteCuentasPorCobrarDto
    {
        public decimal TotalPendiente { get; set; }
        public int CreditosPendientes { get; set; }
        public decimal MontoVencido { get; set; }
        public int CreditosVencidos { get; set; }
        public List<ReporteCreditoDto> Creditos { get; set; } = new();
    }

    public class ReporteCreditoDto
    {
        public int IdVenta { get; set; }
        public string Cliente { get; set; } = null!;
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public decimal SaldoPendiente { get; set; }
        public DateOnly? FechaLimitePago { get; set; }
        public int DiasVencido { get; set; }
    }

    public class ReporteInventarioDto
    {
        public decimal ValorInventario { get; set; }
        public int ProductosStockBajo { get; set; }
        public int LotesPorVencer { get; set; }
        public List<ReporteStockBajoDto> StockBajo { get; set; } = new();
        public List<ReporteLoteVencimientoDto> ProximosVencimientos { get; set; } = new();
    }

    public class ReporteStockBajoDto
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; } = null!;
        public string Sucursal { get; set; } = null!;
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
    }

    public class ReporteLoteVencimientoDto
    {
        public int IdLote { get; set; }
        public string Producto { get; set; } = null!;
        public string NumeroLote { get; set; } = null!;
        public string Sucursal { get; set; } = null!;
        public DateOnly FechaVencimiento { get; set; }
        public decimal CantidadDisponible { get; set; }
        public int DiasRestantes { get; set; }
    }
}
