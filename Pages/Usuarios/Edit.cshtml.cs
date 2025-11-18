using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Usuarios
{
    [Authorize(Policy = "RequireUserRole")]
    public class EditModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IUsuarioService usuarioService, ILogger<EditModel> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [BindProperty]
        public UsuarioEditDTO Usuario { get; set; } = new UsuarioEditDTO();

        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                
                if (usuario == null)
                {
                    return NotFound();
                }

                Usuario = new UsuarioEditDTO
                {
                    NombreUsuario = usuario.NombreUsuario,
                    Correo = usuario.Correo,
                    IdRol = usuario.IdRol,
                    Activo = usuario.Activo
                };

                await CargarRoles();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuario para edición");
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnGetModalAsync(string id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                
                if (usuario == null)
                {
                    return NotFound();
                }

                Usuario = new UsuarioEditDTO
                {
                    NombreUsuario = usuario.NombreUsuario,
                    Correo = usuario.Correo,
                    IdRol = usuario.IdRol,
                    Activo = usuario.Activo
                };

                await CargarRoles();
                return Partial("_EditPartial", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuario para edición en modal");
                return StatusCode(500, "Error al cargar el usuario");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarRoles();
                return Page();
            }

            try
            {
                var updatedUsuario = await _usuarioService.UpdateUsuarioAsync(Usuario.NombreUsuario, Usuario);
                
                if (updatedUsuario == null)
                {
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el usuario. Por favor, intente nuevamente.");
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

        // Handler para reactivar usuarios inactivos
        public async Task<IActionResult> OnPostReactivateAsync(string id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                
                if (usuario == null)
                {
                    return new JsonResult(new { success = false, message = "Usuario no encontrado" });
                }

                var usuarioEdit = new UsuarioEditDTO
                {
                    NombreUsuario = usuario.NombreUsuario,
                    Correo = usuario.Correo,
                    IdRol = usuario.IdRol,
                    Activo = true, // REACTIVAR
                    CambiarContrasenia = false
                };
                
                var updatedUsuario = await _usuarioService.UpdateUsuarioAsync(id, usuarioEdit);
                
                if (updatedUsuario == null)
                {
                    return new JsonResult(new { success = false, message = "Error al reactivar usuario" });
                }

                _logger.LogInformation("Usuario reactivado exitosamente: {UsuarioId}", id);
                
                return new JsonResult(new { success = true, message = "Usuario reactivado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reactivar usuario: {UsuarioId}", id);
                return new JsonResult(new { success = false, message = "Error al reactivar el usuario" });
            }
        }
    }
}