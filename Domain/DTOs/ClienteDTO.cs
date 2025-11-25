using System.ComponentModel.DataAnnotations;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class ClienteDTO
    {
        public int ClienteID { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }

    public class ClienteCreateDTO
    {
        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener exactamente 8 dígitos")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener solo 8 números (sin letras ni caracteres especiales)")]
        [Display(Name = "DNI")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)*$", ErrorMessage = "El nombre solo puede contener letras y espacios (mínimo 2 caracteres)")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)*$", ErrorMessage = "El apellido solo puede contener letras y espacios (mínimo 2 caracteres)")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El teléfono celular debe tener exactamente 9 dígitos")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "El teléfono debe contener exactamente 9 números (solo celulares)")]
        [Display(Name = "Teléfono Celular")]
        public string? Telefono { get; set; }

        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(?:gmail\.com|outlook\.com|hotmail\.com|yahoo\.com|icloud\.com|live\.com|proton\.me)$", ErrorMessage = "El correo debe pertenecer a dominios confiables (gmail.com, outlook.com, hotmail.com, yahoo.com, icloud.com, live.com, proton.me)")]
        [Display(Name = "Correo Electrónico")]
        public string? Correo { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria para el programa de fidelización")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }
    }

    public class ClienteEditDTO
    {
        public int ClienteID { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener exactamente 8 dígitos")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener solo 8 números (sin letras ni caracteres especiales)")]
        [Display(Name = "DNI")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)*$", ErrorMessage = "El nombre solo puede contener letras y espacios (mínimo 2 caracteres)")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)*$", ErrorMessage = "El apellido solo puede contener letras y espacios (mínimo 2 caracteres)")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El teléfono celular debe tener exactamente 9 dígitos")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "El teléfono debe contener exactamente 9 números (solo celulares)")]
        [Display(Name = "Teléfono Celular")]
        public string? Telefono { get; set; }

        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(?:gmail\.com|outlook\.com|hotmail\.com|yahoo\.com|icloud\.com|live\.com|proton\.me)$", ErrorMessage = "El correo debe pertenecer a dominios confiables (gmail.com, outlook.com, hotmail.com, yahoo.com, icloud.com, live.com, proton.me)")]
        [Display(Name = "Correo Electrónico")]
        public string? Correo { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria para el programa de fidelización")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }
        
        public bool Activo { get; set; }
    }
}