using System.ComponentModel.DataAnnotations;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class TipoEgresoCreateDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;
    }
}
