namespace HerederosDelReyBackend.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public int IdCategoria { get; set; }

        public int? IdMarca { get; set; }

        public string Codigo { get; set; } = null!;

        public string? CodigoBarras { get; set; }

        public string Nombre { get; set; } = null!;

        public string? NombreGenerico { get; set; }

        public string? Descripcion { get; set; }

        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public decimal? Peso { get; set; }

        public bool ControlaInventario { get; set; }

        public bool ManejaLotes { get; set; }

        public bool TieneVencimiento { get; set; }

        public bool PermiteVenta { get; set; }

        public decimal StockActual { get; set; }


    }
    
}
