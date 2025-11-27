using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Egresos
{
    [Authorize(Roles = "Administrador,Admin")]
    public class DeleteModel : PageModel
    {
        private readonly EgresoService _egresoService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(EgresoService egresoService, ILogger<DeleteModel> logger)
        {
            _egresoService = egresoService;
            _logger = logger;
        }

        [BindProperty]
        public EgresoDTO Egreso { get; set; } = new EgresoDTO();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Egreso = await _egresoService.ObtenerPorIdAsync(id);
            
            if (Egreso == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetModalAsync(int id)
        {
            Egreso = await _egresoService.ObtenerPorIdAsync(id);
            
            if (Egreso == null)
            {
                return NotFound();
            }

            _logger.LogInformation("🗑️ Modal eliminar egreso abierto: {EgresoId}", id);
            return Partial("_DeletePartial", Egreso);
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var egreso = await _egresoService.ObtenerPorIdAsync(id);
                
                if (egreso == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return new JsonResult(new { success = false, message = "Egreso no encontrado" })
                        { StatusCode = 404 };
                    }
                    return NotFound();
                }

                var resultado = await _egresoService.EliminarAsync(id);

                if (resultado)
                {
                    _logger.LogInformation("Egreso eliminado exitosamente: {EgresoId}", id);
                    TempData["SuccessMessage"] = "Egreso eliminado correctamente";

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return new JsonResult(new 
                        { 
                            success = true, 
                            message = "Egreso eliminado correctamente" 
                        });
                    }

                    return RedirectToPage("./Index");
                }

                ModelState.AddModelError(string.Empty, "No se pudo eliminar el egreso");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = "No se pudo eliminar el egreso" })
                    { StatusCode = 500 };
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar egreso {EgresoId}", id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al eliminar el egreso");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = ex.Message })
                    { StatusCode = 500 };
                }

                return Page();
            }
        }
    }
}
