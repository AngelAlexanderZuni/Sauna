using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        // No hay columna idUsuario en tu base de datos
        // Usaremos un enfoque diferente para el ID
        
        [Column("nombreUsuario")]
        [Key] // Usamos nombreUsuario como clave primaria
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

        // No hay fechaCreacion en tu base de datos
        [NotMapped]
        public DateTime? FechaRegistro { get; set; }
        
        // Agregamos una propiedad de ID artificial para compatibilidad
        [NotMapped]
        public int UsuarioID { get; set; }
    }
}