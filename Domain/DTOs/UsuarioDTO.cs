using System.ComponentModel.DataAnnotations;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasenia { get; set; } = string.Empty;
    }

    public class UsuarioDTO
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string CorreoElectronico => Correo; // Alias para compatibilidad
        public int IdRol { get; set; }
        public int IdUsuario => 0; // Por compatibilidad (nombreUsuario es la PK)
        public bool Activo { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Objeto Rol para navegación
        public RolInfo? Rol { get; set; }
    }
    
    public class RolInfo
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }

    public class UsuarioCreateDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_\-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, guiones y guiones bajos")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,}$", 
            ErrorMessage = "La contraseña debe contener al menos: 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial (@$!%*?&.)")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasenia { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(?:gmail\.com|outlook\.com|hotmail\.com|yahoo\.com|icloud\.com|live\.com|proton\.me)$", ErrorMessage = "El correo debe pertenecer a dominios confiables (gmail.com, outlook.com, hotmail.com, yahoo.com, icloud.com, live.com, proton.me)")]
        [Display(Name = "Correo Electrónico")]
        public string? Correo { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        public bool Activo { get; set; } = true;
    }

    public class UsuarioEditDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [StringLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(?:gmail\.com|outlook\.com|hotmail\.com|yahoo\.com|icloud\.com|live\.com|proton\.me)$", ErrorMessage = "El correo debe pertenecer a dominios confiables (gmail.com, outlook.com, hotmail.com, yahoo.com, icloud.com, live.com, proton.me)")]
        [Display(Name = "Correo Electrónico")]
        public string? Correo { get; set; }

        [Required(ErrorMessage = "El rol es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        public bool Activo { get; set; }

        public bool CambiarContrasenia { get; set; } = false;

        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,}$", 
            ErrorMessage = "La contraseña debe contener al menos: 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial (@$!%*?&.)")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string? NuevaContrasenia { get; set; }
    }

    public class CambioContraseniaDTO
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string ContraseniaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,}$", 
            ErrorMessage = "La contraseña debe contener al menos: 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial (@$!%*?&.)")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NuevaContrasenia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña")]
        [Compare("NuevaContrasenia", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarContrasenia { get; set; } = string.Empty;
    }
}