using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("TipoDescuento")]
    public class TipoDescuento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("idTipoDescuento")]
        public int IdTipoDescuento { get; set; }

        [Required]
        [StringLength(50)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        // Navegación
        public virtual ICollection<Promocion> Promociones { get; set; } = new List<Promocion>();
    }
}
