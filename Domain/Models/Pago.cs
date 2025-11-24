using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Pago")]
    public class Pago
    {
        [Key]
        [Column("idPago")]
        public int IdPago { get; set; }

        [Required]
        [Column("fechaHora")]
        public DateTime FechaHora { get; set; }

        [Required]
        [Column("monto", TypeName = "decimal(12,2)")]
        public decimal Monto { get; set; }

        [StringLength(100)]
        [Column("numeroReferencia")]
        public string? NumeroReferencia { get; set; }

        [Required]
        [Column("idMetodoPago")]
        public int IdMetodoPago { get; set; }

        [Required]
        [Column("idCuenta")]
        public int IdCuenta { get; set; }

        [ForeignKey("IdMetodoPago")]
        public virtual MetodoPago MetodoPago { get; set; } = null!;

        [ForeignKey("IdCuenta")]
        public virtual Cuenta Cuenta { get; set; } = null!;
    }
}