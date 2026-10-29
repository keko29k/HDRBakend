namespace HerederosDelReyBackend.DTOs.DTO
{
    public class InventarioSucursalListadoDto
    {
        public int Id { get; set; }
        public int IdSucursal { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = null!;
        public string NombreSucursal { get; set; } = null!;
        public string? Ubicacion { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal? StockMaximo { get; set; }
        public string EstadoStock { get; set; } = null!;
    }
}
