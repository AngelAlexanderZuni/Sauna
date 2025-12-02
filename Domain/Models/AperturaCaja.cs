using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("AperturaCaja")]
    public class AperturaCaja
    {
        [Key]
        [Column("idApertura")]
        public int IdApertura { get; set; }

        [Required]
        [Column("fechaHoraApertura")]
        public DateTime FechaHoraApertura { get; set; }

        [Required]
        [Column("saldoInicial", TypeName = "decimal(12,2)")]
        public decimal SaldoInicial { get; set; }

        [Required]
        [Column("fondoCambio", TypeName = "decimal(12,2)")]
        public decimal FondoCambio { get; set; }

        [StringLength(500)]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Required]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;

        public virtual CierreCaja? Cierre { get; set; }
    }
}
