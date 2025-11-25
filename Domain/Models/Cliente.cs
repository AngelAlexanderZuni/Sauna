using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoSaunaKalixto.Web.Domain.Models
{
    [Table("Cliente")]
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("idCliente")]
        public int ClienteID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("apellidos")]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("numero_documento")]
        public string? NumeroDocumento { get; set; }

        [StringLength(50)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [StringLength(100)]
        [Column("correo")]
        public string? Correo { get; set; }

        [StringLength(200)]
        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("fechaNacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Column("visitasTotales")]
        public int VisitasTotales { get; set; } = 0;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        // Propiedades adicionales para compatibilidad
        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";
        
        [NotMapped]
        public string? TipoMembresia { get; set; }
        
        [NotMapped]
        public DateTime? FechaInicioMembresia { get; set; }
        
        [NotMapped]
        public DateTime? FechaFinMembresia { get; set; }
    }
}