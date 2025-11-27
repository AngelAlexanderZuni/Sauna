using System.ComponentModel.DataAnnotations;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class EgresoDTO
    {
        public int IdCabEgreso { get; set; }
        public DateTime Fecha { get; set; }
        public decimal MontoTotal { get; set; }
        public int? IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public List<DetalleEgresoDTO> Detalles { get; set; } = new List<DetalleEgresoDTO>();
    }

    public class DetalleEgresoDTO
    {
        public int IdDetEgreso { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public bool Recurrente { get; set; }
        public string? ComprobanteRuta { get; set; }
        public int IdTipoEgreso { get; set; }
        public string? NombreTipoEgreso { get; set; }
    }

    public class EgresoCreateDTO
    {
        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Debe agregar al menos un detalle")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un detalle")]
        public List<DetalleEgresoCreateDTO> Detalles { get; set; } = new List<DetalleEgresoCreateDTO>();
    }

    public class DetalleEgresoCreateDTO
    {
        [Required(ErrorMessage = "El concepto es requerido")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "El concepto debe tener entre 3 y 200 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s\.,\-\(\)]+$", ErrorMessage = "El concepto contiene caracteres no válidos")]
        public string Concepto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe estar entre S/ 0.01 y S/ 999,999.99")]
        public decimal Monto { get; set; }

        public bool Recurrente { get; set; }

        [StringLength(80, ErrorMessage = "La ruta del comprobante no puede exceder 80 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_\.\/\\#]+$", ErrorMessage = "El comprobante contiene caracteres no válidos")]
        public string? ComprobanteRuta { get; set; }

        [Required(ErrorMessage = "El tipo de egreso es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de egreso válido")]
        public int IdTipoEgreso { get; set; }
    }

    public class EgresoFilterDTO
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? IdTipoEgreso { get; set; }
        public decimal? MontoMinimo { get; set; }
        public decimal? MontoMaximo { get; set; }
    }
}
