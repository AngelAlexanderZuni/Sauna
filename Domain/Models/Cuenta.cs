using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Cuenta")]
    public class Cuenta
    {
        [Key]
        [Column("idCuenta")]
        public int IdCuenta { get; set; }

        [Required]
        [Column("fechaHoraCreacion")]
        public DateTime FechaHoraCreacion { get; set; }

        [Column("fechaHoraSalida")]
        public DateTime? FechaHoraSalida { get; set; }

        [Required]
        [Column("subtotalConsumos", TypeName = "decimal(18,2)")]
        public decimal SubtotalConsumos { get; set; }

        [Required]
        [Column("descuento", TypeName = "decimal(18,2)")]
        public decimal Descuento { get; set; }

        [Required]
        [Column("total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column("idEstadoCuenta")]
        public int IdEstadoCuenta { get; set; }

        [Column("idUsuarioCreador")]
        public int IdUsuarioCreador { get; set; }

        [Column("idCliente")]
        public int IdCliente { get; set; }

        [ForeignKey("IdUsuarioCreador")]
        public virtual Usuario UsuarioCreador { get; set; } = null!;

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; } = null!;

        [ForeignKey("IdEstadoCuenta")]
        public virtual EstadoCuenta EstadoCuenta { get; set; } = null!;

        public virtual ICollection<DetalleServicio> DetallesServicio { get; set; } = new List<DetalleServicio>();
        public virtual ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}