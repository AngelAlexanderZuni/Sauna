using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("EstadoCuenta")]
    public class EstadoCuenta
    {
        [Key]
        [Column("idEstadoCuenta")]
        public int IdEstadoCuenta { get; set; }

        [Required]
        [StringLength(30)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}