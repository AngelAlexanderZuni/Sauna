using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("nombreUsuario")]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [Column("contraseniaHash")]
        public string ContraseniaHash { get; set; } = string.Empty;

        [Column("idRol")]
        public int IdRol { get; set; }

        [NotMapped]
        public string Rol { get; set; } = string.Empty;

        [StringLength(100)]
        [NotMapped]
        public string? NombreCompleto { get; set; }

        [StringLength(150)]
        [Column("correo")]
        public string? Correo { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [NotMapped]
        public DateTime? FechaRegistro { get; set; }
        
        // Propiedad de navegación
        [NotMapped]
        public string Nombre => NombreCompleto ?? NombreUsuario;
    }
}