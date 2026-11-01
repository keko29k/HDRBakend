namespace HerederosDelReyBackend.DTOs.DTO_CREATE
{
    public class RegistrarAbonoCreditoDto
    {
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = null!;
        public string? Referencia { get; set; }
        public string? Observacion { get; set; }
    }
}