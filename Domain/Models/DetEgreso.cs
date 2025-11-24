using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("DetEgreso")]
    public class DetEgreso
    {
        [Key]
        [Column("idDetEgreso")]
        public int IdDetEgreso { get; set; }

        [Column("idCabEgreso")]
        public int? IdCabEgreso { get; set; }

        [Required]
        [StringLength(200)]
        [Column("concepto")]
        public string Concepto { get; set; } = string.Empty;

        [Required]
        [Column("monto", TypeName = "decimal(12,2)")]
        public decimal Monto { get; set; }

        [Required]
        [Column("recurrente")]
        public bool Recurrente { get; set; }

        [StringLength(80)]
        [Column("comprobanteRuta")]
        public string? ComprobanteRuta { get; set; }

        [Required]
        [Column("idTipoEgreso")]
        public int IdTipoEgreso { get; set; }

        [ForeignKey("IdCabEgreso")]
        public virtual CabEgreso? CabEgreso { get; set; }

        [ForeignKey("IdTipoEgreso")]
        public virtual TipoEgreso TipoEgreso { get; set; } = null!;
    }
}