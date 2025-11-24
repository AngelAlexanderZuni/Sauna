using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("ProgramaFidelizacion")]
    public class ProgramaFidelizacion
    {
        [Key]
        [Column("idPrograma")]
        public int IdPrograma { get; set; }

        [Required]
        [Column("visitasParaDescuento")]
        public int VisitasParaDescuento { get; set; }

        [Required]
        [Column("porcentajeDescuento", TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; }

        [Required]
        [Column("descuentoCumpleanos")]
        public bool DescuentoCumpleanos { get; set; }

        [Required]
        [Column("montoDescuentoCumpleanos", TypeName = "decimal(12,2)")]
        public decimal MontoDescuentoCumpleanos { get; set; }
    }
}