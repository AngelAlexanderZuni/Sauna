using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("TipoComprobante")]
    public class TipoComprobante
    {
        [Key]
        [Column("idTipoComprobante")]
        public int IdTipoComprobante { get; set; }

        [Required]
        [StringLength(30)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}