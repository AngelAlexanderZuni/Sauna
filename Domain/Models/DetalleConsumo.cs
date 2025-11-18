using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("DetalleConsumo")]
    public class DetalleConsumo
    {
        [Key]
        [Column("idDetalle")]
        public int IdDetalle { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("precioUnitario", TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column("idOrden")]
        public int? IdOrden { get; set; }

        [Column("idProducto")]
        public int IdProducto { get; set; }

        // Navegación
        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; } = null!;

        // Propiedades calculadas
        [NotMapped]
        public decimal Total => Cantidad * PrecioUnitario;
    }
}