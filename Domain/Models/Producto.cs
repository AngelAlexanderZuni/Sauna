using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Producto")]
    public class Producto
    {
        [Key]
        [Column("idProducto")]
        public int IdProducto { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("precioCompra", TypeName = "decimal(18,2)")]
        public decimal PrecioCompra { get; set; }

        [Required]
        [Column("precioVenta", TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }

        [Required]
        [Column("stockActual")]
        public int StockActual { get; set; }

        [Required]
        [Column("stockMinimo")]
        public int StockMinimo { get; set; }

        [Column("idCategoriaProducto")]
        public int? IdCategoriaProducto { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [NotMapped]
        public DateTime? FechaRegistro { get; set; }

        // Navegación
        [ForeignKey("IdCategoriaProducto")]
        public virtual CategoriaProducto? CategoriaProducto { get; set; }

        // Relación con DetalleConsumo
        public virtual ICollection<DetalleConsumo> DetallesConsumo { get; set; } = new List<DetalleConsumo>();

        // Propiedades calculadas
        [NotMapped]
        public decimal MargenBeneficio => PrecioVenta > 0 && PrecioCompra > 0 
            ? ((PrecioVenta - PrecioCompra) / PrecioCompra) * 100 
            : 0;

        [NotMapped]
        public bool StockBajo => StockActual <= StockMinimo;

        [NotMapped]
        public string EstadoStock => StockActual == 0 ? "Sin Stock" 
            : StockBajo ? "Stock Bajo" 
            : "Normal";
    }
}
