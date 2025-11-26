using System.ComponentModel.DataAnnotations;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class PromocionDto
    {
        public int IdPromocion { get; set; }
        public string NombreDescuento { get; set; } = string.Empty;
        public decimal MontoDescuento { get; set; }
        public int? ValorCondicion { get; set; }
        public bool Activo { get; set; }
        public string? Motivo { get; set; }
        public int IdTipoDescuento { get; set; }
        public string TipoNombre { get; set; } = string.Empty;
    }

    public class PromocionCreateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string NombreDescuento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoDescuento { get; set; }

        [Required(ErrorMessage = "Seleccione un tipo de descuento")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo válido")]
        public int IdTipoDescuento { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La condición debe ser positiva")]
        public int? ValorCondicion { get; set; }

        public bool Activo { get; set; } = true;

        [StringLength(200, ErrorMessage = "El motivo no puede exceder 200 caracteres")]
        public string? Motivo { get; set; }
    }

    public class PromocionEditDto
    {
        public int IdPromocion { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string NombreDescuento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoDescuento { get; set; }

        [Required(ErrorMessage = "Seleccione un tipo de descuento")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo válido")]
        public int IdTipoDescuento { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La condición debe ser positiva")]
        public int? ValorCondicion { get; set; }

        public bool Activo { get; set; } = true;

        [StringLength(200, ErrorMessage = "El motivo no puede exceder 200 caracteres")]
        public string? Motivo { get; set; }
    }

    public class TipoDescuentoDto
    {
        public int IdTipoDescuento { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}