using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("DetalleServicio")]
    public class DetalleServicio
    {
        [Key]
        [Column("idDetalleServicio")]
        public int IdDetalleServicio { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("precioUnitario", TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column("idCuenta")]
        public int IdCuenta { get; set; }

        [Required]
        [Column("idServicio")]
        public int IdServicio { get; set; }

        [ForeignKey("IdCuenta")]
        public virtual Cuenta Cuenta { get; set; } = null!;

        [ForeignKey("IdServicio")]
        public virtual Servicio Servicio { get; set; } = null!;
    }
}