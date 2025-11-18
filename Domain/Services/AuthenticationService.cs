using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Helpers;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IAuthService
    {
        Task<UsuarioDTO?> AuthenticateAsync(LoginDTO loginDTO);
        Task<UsuarioDTO?> GetUsuarioByIdAsync(string id);
    }

    public class AuthenticationService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private static readonly Dictionary<string, LoginAttempt> _loginAttempts = new();
        private static readonly object _lockObject = new();
        private const int MaxLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public AuthenticationService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        private class LoginAttempt
        {
            public int Attempts { get; set; }
            public DateTime LastAttempt { get; set; }
            public DateTime? LockoutUntil { get; set; }
        }

        public async Task<UsuarioDTO?> AuthenticateAsync(LoginDTO loginDTO)
        {
            // Sanitización de entrada
            var nombreUsuario = loginDTO.NombreUsuario?.Trim() ?? string.Empty;

            // Verificar lockout
            if (IsLockedOut(nombreUsuario))
            {
                Console.WriteLine($"Usuario bloqueado temporalmente: {nombreUsuario}");
                await Task.Delay(1000); // Prevenir timing attacks
                return null;
            }

            var usuario = await _usuarioRepository.GetByIdAsync(nombreUsuario);
            
            // Siempre hacer la verificación de contraseña aunque el usuario no exista
            // para prevenir timing attacks
            bool passwordValid = false;
            string hashToVerify = usuario?.ContraseniaHash ?? 
                "$2a$11$dummy.hash.to.prevent.timing.attacks.XXXXXXXXXXXXXXXXXXXX";
            
            passwordValid = PasswordHelper.VerifyPassword(loginDTO.Contrasenia, hashToVerify);

            if (usuario == null || !usuario.Activo || !passwordValid)
            {
                RegisterFailedAttempt(nombreUsuario);
                Console.WriteLine($"Autenticación fallida para: {nombreUsuario}");
                await Task.Delay(1000); // Prevenir timing attacks y enumeration attacks
                return null;
            }

            // Autenticación exitosa - limpiar intentos
            ClearLoginAttempts(nombreUsuario);

            // Mapear idRol a nombre de rol
            string rolNombre = usuario.IdRol switch
            {
                1 => "Administrador",
                5 => "Administrador",  // Admin tiene IdRol 5 en la BD actual
                2 => "Cajero",
                _ => "Cajero"
            };

            Console.WriteLine($"Usuario autenticado exitosamente: {usuario.NombreUsuario}, Rol: {rolNombre}");

            return new UsuarioDTO
            {
                NombreUsuario = usuario.NombreUsuario,
                RolNombre = rolNombre,
                IdRol = usuario.IdRol,
                Correo = usuario.Correo ?? string.Empty,
                Activo = usuario.Activo
            };
        }

        private bool IsLockedOut(string nombreUsuario)
        {
            lock (_lockObject)
            {
                if (_loginAttempts.TryGetValue(nombreUsuario, out var attempt))
                {
                    if (attempt.LockoutUntil.HasValue && attempt.LockoutUntil.Value > DateTime.Now)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private void RegisterFailedAttempt(string nombreUsuario)
        {
            lock (_lockObject)
            {
                if (!_loginAttempts.TryGetValue(nombreUsuario, out var attempt))
                {
                    attempt = new LoginAttempt();
                    _loginAttempts[nombreUsuario] = attempt;
                }

                attempt.Attempts++;
                attempt.LastAttempt = DateTime.Now;

                if (attempt.Attempts >= MaxLoginAttempts)
                {
                    attempt.LockoutUntil = DateTime.Now.Add(LockoutDuration);
                    Console.WriteLine($"Usuario bloqueado por {LockoutDuration.TotalMinutes} minutos: {nombreUsuario}");
                }
            }
        }

        private void ClearLoginAttempts(string nombreUsuario)
        {
            lock (_lockObject)
            {
                _loginAttempts.Remove(nombreUsuario);
            }
        }

        public async Task<UsuarioDTO?> GetUsuarioByIdAsync(string id)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null || !usuario.Activo)
            {
                return null;
            }

            // Mapear idRol a nombre de rol
            string rolNombre = usuario.IdRol switch
            {
                1 => "Administrador",
                5 => "Administrador",  // Admin tiene IdRol 5 en la BD actual
                2 => "Cajero",
                _ => "Cajero"
            };

            return new UsuarioDTO
            {
                NombreUsuario = usuario.NombreUsuario,
                RolNombre = rolNombre,
                IdRol = usuario.IdRol,
                Correo = usuario.Correo ?? string.Empty,
                Activo = usuario.Activo
            };
        }
    }
}