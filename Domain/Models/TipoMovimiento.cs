using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("TipoMovimiento")]
    public class TipoMovimiento
    {
        [Key]
        [Column("idTipoMovimiento")]
        public int IdTipoMovimiento { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("tipo")]
        public string Nombre { get; set; } = string.Empty;

        public virtual ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    }
}