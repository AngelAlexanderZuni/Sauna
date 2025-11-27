using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("CabEgreso")]
    public class CabEgreso
    {
        [Key]
        [Column("idCabEgreso")]
        public int IdCabEgreso { get; set; }

        [Required]
        [Column("fecha", TypeName = "datetime2(0)")]
        public DateTime Fecha { get; set; }

        [Column("montoTotal", TypeName = "decimal(12,2)")]
        public decimal MontoTotal { get; set; }

        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<DetEgreso> DetallesEgreso { get; set; } = new List<DetEgreso>();
    }
}