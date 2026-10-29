namespace HerederosDelReyBackend.DTOs
{
    public class VentaDto
    {
        public int Id { get; set; }

        public int IdSucursal { get; set; }

        public int? IdCliente { get; set; }

        public int IdUsuario { get; set; }

        public bool Acredito { get; set; }

        public decimal MontoPagado { get; set; }

        public decimal SaldoPendiente { get; set; }

        public DateOnly? FechaLimitePago { get; set; }

        
        public DateTime FechaVenta { get; set; }

        
        public decimal Total { get; set; }

        public string? Observacion { get; set; }

        public string EstadoVenta { get; set; } = null!;

    }
}
