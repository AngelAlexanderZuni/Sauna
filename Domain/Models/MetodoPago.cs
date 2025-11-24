using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("MetodoPago")]
    public class MetodoPago
    {
        [Key]
        [Column("idMetodoPago")]
        public int IdMetodoPago { get; set; }

        [Required]
        [StringLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}