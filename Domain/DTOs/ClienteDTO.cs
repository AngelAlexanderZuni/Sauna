using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class ClienteDto
    {
        public int ClienteID { get; set; }
        public int IdCliente { get => ClienteID; }
        public string? NumeroDocumento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string NombreCompleto => (Nombre + " " + Apellido).Trim();

        public static ClienteDto FromEntity(Cliente c)
        {
            return new ClienteDto
            {
                ClienteID = c.ClienteID,
                NumeroDocumento = c.NumeroDocumento,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Telefono = c.Telefono,
                Correo = c.Correo,
                Direccion = c.Direccion,
                FechaNacimiento = c.FechaNacimiento,
                Activo = c.Activo,
                FechaRegistro = c.FechaRegistro
            };
        }
    }

    public class ClienteDTO
    {
        public int ClienteID { get; set; }
        public int IdCliente { get => ClienteID; }
        public string? NumeroDocumento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string NombreCompleto => (Nombre + " " + Apellido).Trim();
    }

    public class ClienteCreateDTO
    {
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }

    public class ClienteEditDTO
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
    }
}

