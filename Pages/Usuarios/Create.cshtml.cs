using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Usuarios
{
    [Authorize(Policy = "RequireUserRole")]
    public class CreateModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IUsuarioService usuarioService, ILogger<CreateModel> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [BindProperty]
        public UsuarioCreateDTO Usuario { get; set; } = new UsuarioCreateDTO();

        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            await CargarRoles();
        }

        public async Task<IActionResult> OnGetModal()
        {
            await CargarRoles();
            return Partial("_CreatePartial", this);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarRoles();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 422;
                    return Partial("_CreatePartial", this);
                }
                return Page();
            }

            try
            {
                // Verificar si ya existe un usuario con el mismo nombre
                if (await _usuarioService.ExistsByNombreUsuarioAsync(Usuario.NombreUsuario))
                {
                    ModelState.AddModelError("Usuario.NombreUsuario", "Ya existe un usuario con este nombre.");
                    await CargarRoles();
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        Response.StatusCode = 422;
                        return Partial("_CreatePartial", this);
                    }
                    return Page();
                }

                await _usuarioService.CreateUsuarioAsync(Usuario);
                
                TempData["SuccessMessage"] = "Usuario creado exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación al crear usuario");
                var message = ex.Message ?? "Error de validación";
                if (message.Contains("rol", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Usuario.IdRol", message);
                }
                else if (message.Contains("contraseñ", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Usuario.Contrasenia", message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                }
                await CargarRoles();
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 422;
                    return Partial("_CreatePartial", this);
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el usuario. Por favor, intente nuevamente.");
                await CargarRoles();
                return Page();
            }
        }

        private async Task CargarRoles()
        {
            var roles = await _usuarioService.GetAllRolesAsync();
            Roles = roles.Select(r => new SelectListItem
            {
                Value = r.IdRol.ToString(),
                Text = r.Nombre
            }).ToList();
        }
    }
}