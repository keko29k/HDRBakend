namespace HerederosDelReyBackend.DTOs.DTO
{
    public class InventarioSucursalResumenDto
    {
        public decimal UnidadesEnInventario { get; set; }
        public int StockBajo { get; set; }
        public int SinExistencias { get; set; }
        public int LotesPorVencer { get; set; }
        public List<InventarioSucursalListadoDto> ProductosStockBajo { get; set; } = new();
        public List<LotePorVencerDto> DetalleLotesPorVencer { get; set; } = new();
    }

    public class LotePorVencerDto
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = null!;
        public int IdSucursal { get; set; }
        public string NombreSucursal { get; set; } = null!;
        public string NumeroLote { get; set; } = null!;
        public DateOnly FechaVencimiento { get; set; }
        public decimal CantidadDisponible { get; set; }
        public int DiasParaVencer { get; set; }
    }
}
