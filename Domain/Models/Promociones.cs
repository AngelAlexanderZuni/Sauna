using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Promociones")]
    public class Promociones
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("idPromocion")]
        public int IdPromocion { get; set; }

        [Required]
        [StringLength(200)]
        [Column("nombreDescuento")]
        public string NombreDescuento { get; set; } = string.Empty;

        [Required]
        [Column("montoDescuento", TypeName = "decimal(10,2)")]
        public decimal MontoDescuento { get; set; }

        [Required]
        [Column("idTipoDescuento")]
        public int IdTipoDescuento { get; set; }

        [Required]
        [Column("valorCondicion", TypeName = "decimal(10,2)")]
        public decimal ValorCondicion { get; set; }

        [Required]
        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Required]
        [Column("motivo")]
        public string Motivo { get; set; } = string.Empty;

        // Navegación
        [ForeignKey("IdTipoDescuento")]
        public virtual TipoDescuento? TipoDescuento { get; set; }

        public virtual ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
    }
}
