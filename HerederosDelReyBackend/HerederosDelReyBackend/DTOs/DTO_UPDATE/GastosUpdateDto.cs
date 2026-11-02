namespace HerederosDelReyBackend.DTOs
{
    public class GastosUpdateDto
    {
        
        
        public string Concepto { get; set; } = null!;

        public string? NumeroComprobante { get; set; }

        public decimal Monto { get; set; }

        public string? Observacion { get; set; }
    }
}
