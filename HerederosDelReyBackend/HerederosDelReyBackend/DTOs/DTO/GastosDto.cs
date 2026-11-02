namespace HerederosDelReyBackend.DTOs
{
    public class GastosDto
    {
        public int Id { get; set; }
        public int IdCaja { get; set; }

        public int IdUsuario { get; set; }

        public string Concepto { get; set; } = null!;

        public string? NumeroComprobante { get; set; }

        public decimal Monto { get; set; }

        public string? Observacion { get; set; }

        // Solo lectura: fecha en que se registró el gasto.
        public DateTime FechaRegistro { get; set; }
    }
}
