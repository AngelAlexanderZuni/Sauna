using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Helpers;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDTO>> GetAllUsuariosAsync();
        Task<UsuarioDTO?> GetUsuarioByIdAsync(string nombreUsuario);
        Task<UsuarioDTO> CreateUsuarioAsync(UsuarioCreateDTO usuarioDTO);
        Task<UsuarioDTO?> UpdateUsuarioAsync(string nombreUsuario, UsuarioEditDTO usuarioDTO);
        Task<bool> DeleteUsuarioAsync(string nombreUsuario);
        Task<bool> ExistsByNombreUsuarioAsync(string nombreUsuario);
        Task<bool> ChangePasswordAsync(string nombreUsuario, CambioContraseniaDTO cambioDTO);
        Task<IEnumerable<Rol>> GetAllRolesAsync();
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository, IRolRepository rolRepository)
        {
            _usuarioRepository = usuarioRepository;
            _rolRepository = rolRepository;
        }

        public async Task<IEnumerable<UsuarioDTO>> GetAllUsuariosAsync()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            var roles = await _rolRepository.GetAllAsync();
            var rolesDict = roles.ToDictionary(r => r.IdRol, r => r.Nombre);

            return usuarios.Select(u => new UsuarioDTO
            {
                NombreUsuario = u.NombreUsuario,
                Correo = u.Correo ?? string.Empty,
                IdRol = u.IdRol,
                Activo = u.Activo,
                RolNombre = rolesDict.GetValueOrDefault(u.IdRol, "Desconocido"),
                Rol = new RolInfo
                {
                    IdRol = u.IdRol,
                    NombreRol = rolesDict.GetValueOrDefault(u.IdRol, "Desconocido")
                },
                FechaCreacion = DateTime.Now
            });
        }

        public async Task<UsuarioDTO?> GetUsuarioByIdAsync(string nombreUsuario)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(nombreUsuario);
            if (usuario == null) return null;

            var rol = await _rolRepository.GetByIdAsync(usuario.IdRol);

            return new UsuarioDTO
            {
                NombreUsuario = usuario.NombreUsuario,
                Correo = usuario.Correo ?? string.Empty,
                IdRol = usuario.IdRol,
                Activo = usuario.Activo,
                RolNombre = rol?.Nombre ?? "Desconocido",
                Rol = new RolInfo
                {
                    IdRol = usuario.IdRol,
                    NombreRol = rol?.Nombre ?? "Desconocido"
                },
                FechaCreacion = DateTime.Now
            };
        }

        public async Task<UsuarioDTO> CreateUsuarioAsync(UsuarioCreateDTO usuarioDTO)
        {
            // Sanitización de datos
            usuarioDTO.NombreUsuario = usuarioDTO.NombreUsuario?.Trim() ?? string.Empty;
            usuarioDTO.Correo = usuarioDTO.Correo?.Trim().ToLower();

            // Validar contraseña segura
            ValidarContraseniaSegura(usuarioDTO.Contrasenia);

            // Validar rol existente
            var rolValido = await _rolRepository.GetByIdAsync(usuarioDTO.IdRol);
            if (rolValido == null)
            {
                throw new ArgumentException("Debe seleccionar un rol válido");
            }

            var usuario = new Usuario
            {
                NombreUsuario = usuarioDTO.NombreUsuario,
                ContraseniaHash = PasswordHelper.HashPassword(usuarioDTO.Contrasenia),
                Correo = usuarioDTO.Correo,
                IdRol = usuarioDTO.IdRol,
                Activo = usuarioDTO.Activo
            };

            var createdUsuario = await _usuarioRepository.AddAsync(usuario);
            var rol = await _rolRepository.GetByIdAsync(createdUsuario.IdRol);

            return new UsuarioDTO
            {
                NombreUsuario = createdUsuario.NombreUsuario,
                Correo = createdUsuario.Correo ?? string.Empty,
                IdRol = createdUsuario.IdRol,
                Activo = createdUsuario.Activo,
                RolNombre = rol?.Nombre ?? "Desconocido"
            };
        }

        private void ValidarContraseniaSegura(string contrasenia)
        {
            if (string.IsNullOrWhiteSpace(contrasenia))
            {
                throw new ArgumentException("La contraseña no puede estar vacía");
            }

            if (contrasenia.Length < 8)
            {
                throw new ArgumentException("La contraseña debe tener al menos 8 caracteres");
            }

            if (!contrasenia.Any(char.IsUpper))
            {
                throw new ArgumentException("La contraseña debe contener al menos una letra mayúscula");
            }

            if (!contrasenia.Any(char.IsLower))
            {
                throw new ArgumentException("La contraseña debe contener al menos una letra minúscula");
            }

            if (!contrasenia.Any(char.IsDigit))
            {
                throw new ArgumentException("La contraseña debe contener al menos un número");
            }

            if (!contrasenia.Any(c => "@$!%*?&.".Contains(c)))
            {
                throw new ArgumentException("La contraseña debe contener al menos un carácter especial (@$!%*?&.)");
            }

            // Validar contraseñas comunes
            var contrasenasComunes = new[] { "Password1!", "Qwerty123!", "Admin123!", "12345678*", "Abc123456!" };
            if (contrasenasComunes.Any(c => c.Equals(contrasenia, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("La contraseña es demasiado común, por favor elige una más segura");
            }
        }

        public async Task<UsuarioDTO?> UpdateUsuarioAsync(string nombreUsuario, UsuarioEditDTO usuarioDTO)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(nombreUsuario);
            if (usuario == null) return null;

            // Sanitización de datos
            usuarioDTO.Correo = usuarioDTO.Correo?.Trim().ToLower();

            // Validar rol existente
            var rolValido = await _rolRepository.GetByIdAsync(usuarioDTO.IdRol);
            if (rolValido == null)
            {
                throw new ArgumentException("Debe seleccionar un rol válido");
            }

            usuario.Correo = usuarioDTO.Correo;
            usuario.IdRol = usuarioDTO.IdRol;
            usuario.Activo = usuarioDTO.Activo;

            if (usuarioDTO.CambiarContrasenia && !string.IsNullOrEmpty(usuarioDTO.NuevaContrasenia))
            {
                // Validar contraseña segura
                ValidarContraseniaSegura(usuarioDTO.NuevaContrasenia);
                usuario.ContraseniaHash = PasswordHelper.HashPassword(usuarioDTO.NuevaContrasenia);
            }

            await _usuarioRepository.UpdateAsync(usuario);
            var rol = await _rolRepository.GetByIdAsync(usuario.IdRol);

            return new UsuarioDTO
            {
                NombreUsuario = usuario.NombreUsuario,
                Correo = usuario.Correo ?? string.Empty,
                IdRol = usuario.IdRol,
                Activo = usuario.Activo,
                RolNombre = rol?.Nombre ?? "Desconocido"
            };
        }

        public async Task<bool> DeleteUsuarioAsync(string nombreUsuario)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(nombreUsuario);
            if (usuario == null) return false;

            usuario.Activo = false;
            await _usuarioRepository.UpdateAsync(usuario);
            return true;
        }

        public async Task<bool> ExistsByNombreUsuarioAsync(string nombreUsuario)
        {
            return await _usuarioRepository.ExistsByNombreUsuarioAsync(nombreUsuario);
        }

        public async Task<bool> ChangePasswordAsync(string nombreUsuario, CambioContraseniaDTO cambioDTO)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(nombreUsuario);
            if (usuario == null) return false;

            if (!PasswordHelper.VerifyPassword(cambioDTO.ContraseniaActual, usuario.ContraseniaHash))
            {
                return false;
            }

            usuario.ContraseniaHash = PasswordHelper.HashPassword(cambioDTO.NuevaContrasenia);
            await _usuarioRepository.UpdateAsync(usuario);
            return true;
        }

        public async Task<IEnumerable<Rol>> GetAllRolesAsync()
        {
            return await _rolRepository.GetAllAsync();
        }
    }
}