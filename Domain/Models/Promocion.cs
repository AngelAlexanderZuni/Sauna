using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Promociones")]
    public class Promocion
    {
        [Key]
        [Column("idPromocion")]
        public int IdPromocion { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombreDescuento")]
        public string NombreDescuento { get; set; } = string.Empty;

        [Required]
        [Column("montoDescuento", TypeName = "decimal(10,2)")]
        public decimal MontoDescuento { get; set; }

        [Column("valorCondicion")]
        public int? ValorCondicion { get; set; }

        [Required]
        [Column("activo")]
        public bool Activo { get; set; } = true;

        [MaxLength(200)]
        [Column("motivo")]
        public string? Motivo { get; set; }

        [Column("idTipoDescuento")]
        public int IdTipoDescuento { get; set; }

        public virtual TipoDescuento? TipoDescuento { get; set; }

        public virtual ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
    }
}

