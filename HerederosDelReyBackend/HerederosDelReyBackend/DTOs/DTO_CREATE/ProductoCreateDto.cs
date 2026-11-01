
using System.ComponentModel.DataAnnotations;

namespace HerederosDelReyBackend.DTOs
{
    public class ProductoCreateDto : IValidatableObject
    {
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

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (ManejaLotes && !ControlaInventario)
            {
                yield return new ValidationResult(
                    "Un producto que maneja lotes debe controlar inventario.",
                    new[] { nameof(ManejaLotes), nameof(ControlaInventario) }
                );
            }

            if (TieneVencimiento && !ManejaLotes)
            {
                yield return new ValidationResult(
                    "Un producto con vencimiento debe manejar lotes.",
                    new[] { nameof(TieneVencimiento), nameof(ManejaLotes) }
                );
            }
        }
    }
}
