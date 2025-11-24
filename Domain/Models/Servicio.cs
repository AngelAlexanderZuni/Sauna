using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Servicio")]
    public class Servicio
    {
        [Key]
        [Column("idServicio")]
        public int IdServicio { get; set; }

        [Required]
        [StringLength(100)]
        [MinLength(3)]
        [RegularExpression("^[A-Za-zÁÉÍÓÚáéíóúñÑüÜ0-9 ]{3,100}$")]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("precio", TypeName = "decimal(12,2)")]
        [Range(typeof(decimal), "0.01", "100000")]
        public decimal Precio { get; set; }

        [Column("duracionEstimada")]
        [Range(1, 600)]
        public int? DuracionEstimada { get; set; }

        [Required]
        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("idCategoriaServicio")]
        public int? IdCategoriaServicio { get; set; }

        [ForeignKey("IdCategoriaServicio")]
        public virtual CategoriaServicio? CategoriaServicio { get; set; }

        public virtual ICollection<DetalleServicio> DetallesServicio { get; set; } = new List<DetalleServicio>();
    }
}