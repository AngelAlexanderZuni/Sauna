using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Clientes
{
    [Authorize(Policy = "RequireClienteAccess")]
    public class DeleteModel : PageModel
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(IClienteService clienteService, ILogger<DeleteModel> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        [BindProperty]
        public ClienteDTO Cliente { get; set; } = new ClienteDTO();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByIdAsync(id);
                
                if (cliente == null)
                {
                    return NotFound();
                }

                Cliente = cliente;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cliente para eliminación");
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnGetModalAsync(int id)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByIdAsync(id);
                
                if (cliente == null)
                {
                    return NotFound();
                }

                Cliente = cliente;
                return Partial("_DeletePartial", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cliente para eliminación en modal");
                return StatusCode(500, "Error al cargar el cliente");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                // Eliminación lógica: solo desactiva el cliente, NO borra los datos
                var result = await _clienteService.DeleteClienteAsync(id);
                
                if (!result)
                {
                    ModelState.AddModelError(string.Empty, "No se encontró el cliente.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var cliente = await _clienteService.GetClienteByIdAsync(id);
                        if (cliente != null) Cliente = cliente;
                        return Partial("_DeletePartial", this);
                    }
                    return NotFound();
                }

                _logger.LogInformation("Cliente desactivado (eliminación lógica): {ClienteId}", id);
                TempData["SuccessMessage"] = "Cliente desactivado exitosamente. Los datos se conservan para reportes y análisis.";
                
                // Si es una petición AJAX, devolver JSON con éxito
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = true, message = "Cliente desactivado exitosamente" });
                }
                
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar cliente");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al desactivar el cliente. Por favor, intente nuevamente.");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var cliente = await _clienteService.GetClienteByIdAsync(id);
                    if (cliente != null) Cliente = cliente;
                    return Partial("_DeletePartial", this);
                }
                return Page();
            }
        }
    }
}