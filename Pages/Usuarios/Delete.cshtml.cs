using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Usuarios
{
    [Authorize(Policy = "RequireUserRole")]
    public class DeleteModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IUsuarioService usuarioService, ILogger<DeleteModel> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [BindProperty]
        public UsuarioDTO Usuario { get; set; } = new UsuarioDTO();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(id);
                
                if (usuario == null)
                {
                    return NotFound();
                }

                Usuario = usuario;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuario para eliminación");
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

                Usuario = usuario;
                return Partial("_DeletePartial", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuario para eliminación en modal");
                return StatusCode(500, "Error al cargar el usuario");
            }
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            try
            {
                var result = await _usuarioService.DeleteUsuarioAsync(id);
                
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Usuario desactivado (eliminación lógica): {UsuarioId}", id);
                TempData["SuccessMessage"] = "Usuario desactivado exitosamente. Los datos se conservan para auditoría y análisis.";
                
                // Soporte AJAX
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = true, message = "Usuario desactivado exitosamente" });
                }

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "Error al desactivar el usuario" });
                }

                ModelState.AddModelError(string.Empty, "Ocurrió un error al desactivar el usuario. Por favor, intente nuevamente.");
                return Page();
            }
        }
    }
}