using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("MovimientoInventario")]
    public class MovimientoInventario
    {
        [Key]
        [Column("idMovimiento")]
        public int IdMovimiento { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("costoUnitario")]
        public decimal CostoUnitario { get; set; }

        [Column("costoTotal")]
        public decimal CostoTotal { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("observaciones")]
        public string? Observacion { get; set; }

        [Column("idTipoMovimiento")]
        public int IdTipoMovimiento { get; set; }

        [Column("idProducto")]
        public int IdProducto { get; set; }

        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        // Propiedades de navegación
        public virtual Producto? Producto { get; set; }
        public virtual TipoMovimiento? TipoMovimiento { get; set; }
    }
}