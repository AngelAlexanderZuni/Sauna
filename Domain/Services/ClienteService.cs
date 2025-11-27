using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteDTO>> ListarAsync(string? busqueda);
        Task<ClienteDTO?> ObtenerAsync(int id);
        Task<IEnumerable<ClienteDTO>> GetAllClientesAsync();
        Task<ClienteDTO?> GetClienteByIdAsync(int id);
        Task<IEnumerable<ClienteDTO>> SearchClientesAsync(string searchTerm);
        Task<IEnumerable<ClienteDTO>> GetClientesByNumeroDocumentoAsync(string numeroDocumento);
        Task<ClienteDTO> CreateClienteAsync(ClienteCreateDTO clienteDTO);
        Task<ClienteDTO?> UpdateClienteAsync(int id, ClienteEditDTO clienteDTO);
        Task<bool> DeleteClienteAsync(int id);
        Task<bool> ExistsByNumeroDocumentoAsync(string numeroDocumento);
    }

    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<IEnumerable<ClienteDTO>> ListarAsync(string? busqueda)
        {
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                return await SearchClientesAsync(busqueda);
            }
            return await GetAllClientesAsync();
        }

        public async Task<ClienteDTO?> ObtenerAsync(int id)
        {
            return await GetClienteByIdAsync(id);
        }

        public async Task<IEnumerable<ClienteDTO>> GetAllClientesAsync()
        {
            var clientes = await _clienteRepository.GetAllAsync();
            return clientes.Select(MapToDTO);
        }

        public async Task<ClienteDTO?> GetClienteByIdAsync(int id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            return cliente != null ? MapToDTO(cliente) : null;
        }

        public async Task<IEnumerable<ClienteDTO>> SearchClientesAsync(string searchTerm)
        {
            var clientes = await _clienteRepository.SearchAsync(searchTerm);
            return clientes.Select(MapToDTO);
        }

        public async Task<IEnumerable<ClienteDTO>> GetClientesByNumeroDocumentoAsync(string numeroDocumento)
        {
            var clientes = await _clienteRepository.GetByNumeroDocumentoAsync(numeroDocumento);
            return clientes.Select(MapToDTO);
        }

        public async Task<ClienteDTO> CreateClienteAsync(ClienteCreateDTO clienteDTO)
        {
            // Validaciones adicionales ANTES de sanitizar
            if (clienteDTO.Nombre.Length < 2)
            {
                throw new ArgumentException("El nombre debe tener al menos 2 caracteres");
            }
            if (clienteDTO.Apellido.Length < 2)
            {
                throw new ArgumentException("El apellido debe tener al menos 2 caracteres");
            }
            
            // Validar que solo contenga letras (antes de sanitizar)
            if (!System.Text.RegularExpressions.Regex.IsMatch(clienteDTO.Nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)*$"))
            {
                throw new ArgumentException("El nombre solo puede contener letras y espacios");
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(clienteDTO.Apellido, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)*$"))
            {
                throw new ArgumentException("El apellido solo puede contener letras y espacios");
            }
            
            // Validar DNI peruano (exactamente 8 dígitos numéricos)
            clienteDTO.NumeroDocumento = clienteDTO.NumeroDocumento?.Trim() ?? string.Empty;
            if (!System.Text.RegularExpressions.Regex.IsMatch(clienteDTO.NumeroDocumento, @"^\d{8}$"))
            {
                throw new ArgumentException("El DNI debe tener exactamente 8 dígitos numéricos");
            }
            
            // Validar teléfono celular peruano (exactamente 9 dígitos) si se proporciona
            if (!string.IsNullOrWhiteSpace(clienteDTO.Telefono))
            {
                clienteDTO.Telefono = clienteDTO.Telefono.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(clienteDTO.Telefono, @"^\d{9}$"))
                {
                    throw new ArgumentException("El teléfono celular debe tener exactamente 9 dígitos numéricos");
                }
            }

            // Sanitización después de validaciones
            clienteDTO.Nombre = SanitizarTexto(clienteDTO.Nombre);
            clienteDTO.Apellido = SanitizarTexto(clienteDTO.Apellido);
            clienteDTO.Correo = clienteDTO.Correo?.Trim().ToLower();
            clienteDTO.Direccion = clienteDTO.Direccion?.Trim();

            // Validación de fecha de nacimiento (OBLIGATORIA para programa de fidelización)
            if (!clienteDTO.FechaNacimiento.HasValue || clienteDTO.FechaNacimiento.Value == default(DateTime))
            {
                throw new ArgumentException("La fecha de nacimiento es obligatoria para el programa de fidelización");
            }
            if (clienteDTO.FechaNacimiento.Value >= DateTime.Now.Date)
            {
                throw new ArgumentException("La fecha de nacimiento debe ser menor a la fecha actual");
            }
            if (clienteDTO.FechaNacimiento.Value.Year < 1900)
            {
                throw new ArgumentException("La fecha de nacimiento no es válida (año debe ser mayor a 1900)");
            }

            var cliente = new Cliente
            {
                NumeroDocumento = clienteDTO.NumeroDocumento,
                Nombre = clienteDTO.Nombre,
                Apellido = clienteDTO.Apellido,
                Telefono = clienteDTO.Telefono,
                Correo = clienteDTO.Correo,
                Direccion = clienteDTO.Direccion,
                FechaNacimiento = clienteDTO.FechaNacimiento,
                Activo = true,
                FechaRegistro = DateTime.Now
            };

            var createdCliente = await _clienteRepository.AddAsync(cliente);
            return MapToDTO(createdCliente);
        }

        private string SanitizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            
            // Trim y capitalizar primera letra de cada palabra
            texto = texto.Trim();
            var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < palabras.Length; i++)
            {
                if (palabras[i].Length > 0)
                {
                    palabras[i] = char.ToUpper(palabras[i][0]) + palabras[i].Substring(1).ToLower();
                }
            }
            return string.Join(" ", palabras);
        }

        public async Task<ClienteDTO?> UpdateClienteAsync(int id, ClienteEditDTO clienteDTO)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                return null;
            }

            // Validaciones adicionales ANTES de sanitizar
            if (clienteDTO.Nombre.Length < 2)
            {
                throw new ArgumentException("El nombre debe tener al menos 2 caracteres");
            }
            if (clienteDTO.Apellido.Length < 2)
            {
                throw new ArgumentException("El apellido debe tener al menos 2 caracteres");
            }
            
            // Validar que solo contenga letras (antes de sanitizar)
            if (!System.Text.RegularExpressions.Regex.IsMatch(clienteDTO.Nombre, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)*$"))
            {
                throw new ArgumentException("El nombre solo puede contener letras y espacios");
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(clienteDTO.Apellido, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+(\s[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)*$"))
            {
                throw new ArgumentException("El apellido solo puede contener letras y espacios");
            }
            
            // Validar DNI peruano (exactamente 8 dígitos numéricos)
            clienteDTO.NumeroDocumento = clienteDTO.NumeroDocumento?.Trim() ?? string.Empty;
            if (!System.Text.RegularExpressions.Regex.IsMatch(clienteDTO.NumeroDocumento, @"^\d{8}$"))
            {
                throw new ArgumentException("El DNI debe tener exactamente 8 dígitos numéricos");
            }
            
            // Validar teléfono celular peruano (exactamente 9 dígitos) si se proporciona
            if (!string.IsNullOrWhiteSpace(clienteDTO.Telefono))
            {
                clienteDTO.Telefono = clienteDTO.Telefono.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(clienteDTO.Telefono, @"^\d{9}$"))
                {
                    throw new ArgumentException("El teléfono celular debe tener exactamente 9 dígitos numéricos");
                }
            }

            // Sanitización después de validaciones
            clienteDTO.Nombre = SanitizarTexto(clienteDTO.Nombre);
            clienteDTO.Apellido = SanitizarTexto(clienteDTO.Apellido);
            clienteDTO.Correo = clienteDTO.Correo?.Trim().ToLower();
            clienteDTO.Direccion = clienteDTO.Direccion?.Trim();

            // Validación de fecha de nacimiento (OBLIGATORIA para programa de fidelización)
            if (clienteDTO.FechaNacimiento == default(DateTime))
            {
                throw new ArgumentException("La fecha de nacimiento es obligatoria para el programa de fidelización");
            }
            if (clienteDTO.FechaNacimiento >= DateTime.Now.Date)
            {
                throw new ArgumentException("La fecha de nacimiento debe ser menor a la fecha actual");
            }
            if (clienteDTO.FechaNacimiento.Value.Year < 1900)
            {
                throw new ArgumentException("La fecha de nacimiento no es válida (año debe ser mayor a 1900)");
            }

            cliente.NumeroDocumento = clienteDTO.NumeroDocumento;
            cliente.Nombre = clienteDTO.Nombre;
            cliente.Apellido = clienteDTO.Apellido;
            cliente.Telefono = clienteDTO.Telefono;
            cliente.Correo = clienteDTO.Correo;
            cliente.Direccion = clienteDTO.Direccion;
            cliente.FechaNacimiento = clienteDTO.FechaNacimiento;
            cliente.Activo = clienteDTO.Activo;

            await _clienteRepository.UpdateAsync(cliente);
            return MapToDTO(cliente);
        }

        public async Task<bool> DeleteClienteAsync(int id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
            {
                return false;
            }

            cliente.Activo = false;
            await _clienteRepository.UpdateAsync(cliente);
            return true;
        }

        public async Task<bool> ExistsByNumeroDocumentoAsync(string numeroDocumento)
        {
            return await _clienteRepository.ExistsByNumeroDocumentoAsync(numeroDocumento);
        }

        private ClienteDTO MapToDTO(Cliente cliente)
        {
            return new ClienteDTO
            {
                ClienteID = cliente.ClienteID,
                NumeroDocumento = cliente.NumeroDocumento,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Telefono = cliente.Telefono,
                Correo = cliente.Correo,
                Direccion = cliente.Direccion,
                FechaNacimiento = cliente.FechaNacimiento,
                Activo = cliente.Activo,
                FechaRegistro = cliente.FechaRegistro
            };
        }
    }
}
