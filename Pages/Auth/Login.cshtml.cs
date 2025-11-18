using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;
using System.Security.Claims;

namespace ProyectoSaunaKalixto.Web.Pages.Auth
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public LoginDTO LoginDTO { get; set; } = new LoginDTO();

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Cerrar sesión si existe alguna cookie de autenticación anterior
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var usuario = await _authService.AuthenticateAsync(LoginDTO);

                if (usuario == null)
                {
                    ErrorMessage = "Usuario o contraseña incorrectos.";
                    return Page();
                }

                // Crear claims para la autenticación
                Console.WriteLine($"Asignando rol: {usuario.RolNombre}");
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.GivenName, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Role, usuario.RolNombre)
                };

                if (!string.IsNullOrEmpty(usuario.Correo))
                {
                    claims.Add(new Claim(ClaimTypes.Email, usuario.Correo));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false, // No persistir entre sesiones del navegador
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("Usuario {Usuario} inició sesión exitosamente", usuario.NombreUsuario);

                return LocalRedirect(returnUrl ?? "/Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar iniciar sesión");
                ErrorMessage = "Ocurrió un error al intentar iniciar sesión. Por favor, intente nuevamente.";
                return Page();
            }
        }
    }
}