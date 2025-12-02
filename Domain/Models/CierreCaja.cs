using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("CierreCaja")]
    public class CierreCaja
    {
        [Key]
        [Column("idCierre")]
        public int IdCierre { get; set; }

        [Required]
        [Column("idApertura")]
        public int IdApertura { get; set; }

        [Required]
        [Column("fechaHoraCierre")]
        public DateTime FechaHoraCierre { get; set; }

        [Required]
        [Column("totalIngresos", TypeName = "decimal(12,2)")]
        public decimal TotalIngresos { get; set; }

        [Required]
        [Column("totalEgresos", TypeName = "decimal(12,2)")]
        public decimal TotalEgresos { get; set; }

        [Required]
        [Column("saldoEsperado", TypeName = "decimal(12,2)")]
        public decimal SaldoEsperado { get; set; }

        [Required]
        [Column("efectivoReal", TypeName = "decimal(12,2)")]
        public decimal EfectivoReal { get; set; }

        [Required]
        [Column("diferencia", TypeName = "decimal(12,2)")]
        public decimal Diferencia { get; set; }

        [StringLength(500)]
        [Column("motivoDiferencia")]
        public string? MotivoDiferencia { get; set; }

        [Column("depositoBancario", TypeName = "decimal(12,2)")]
        public decimal DepositoBancario { get; set; }

        [Column("efectivoEnCaja", TypeName = "decimal(12,2)")]
        public decimal EfectivoEnCaja { get; set; }

        [StringLength(2000)]
        [Column("detalleArqueo")]
        public string? DetalleArqueo { get; set; } // JSON con billetes y monedas

        [Required]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        [ForeignKey("IdApertura")]
        public virtual AperturaCaja Apertura { get; set; } = null!;

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; } = null!;
    }
}
