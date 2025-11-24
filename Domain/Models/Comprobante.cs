using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Comprobante")]
    public class Comprobante
    {
        [Key]
        [Column("idComprobante")]
        public int IdComprobante { get; set; }

        [Required]
        [StringLength(10)]
        [Column("serie")]
        public string Serie { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [Column("numero")]
        public string Numero { get; set; } = string.Empty;

        [Required]
        [Column("fechaEmision")]
        public DateTime FechaEmision { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column("igv", TypeName = "decimal(18,2)")]
        public decimal Igv { get; set; }

        [Required]
        [Column("total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        [Column("idTipoComprobante")]
        public int IdTipoComprobante { get; set; }

        [Required]
        [Column("idCuenta")]
        public int IdCuenta { get; set; }

        [ForeignKey("IdCuenta")]
        public virtual Cuenta Cuenta { get; set; } = null!;

        [ForeignKey("IdTipoComprobante")]
        public virtual TipoComprobante TipoComprobante { get; set; } = null!;
    }
}