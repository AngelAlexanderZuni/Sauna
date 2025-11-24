using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("TipoEgreso")]
    public class TipoEgreso
    {
        [Key]
        [Column("idTipoEgreso")]
        public int IdTipoEgreso { get; set; }

        [Required]
        [StringLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}