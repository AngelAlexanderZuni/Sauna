using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Clientes
{
    [Authorize(Policy = "RequireClienteAccess")]
    public class CreateModel : PageModel
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IClienteService clienteService, ILogger<CreateModel> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        [BindProperty]
        public ClienteCreateDTO Cliente { get; set; } = new ClienteCreateDTO();

        public void OnGet()
        {
        }

        public IActionResult OnGetModal()
        {
            return Partial("_CreatePartial", this);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 422;
                    return Partial("_CreatePartial", this);
                }
                return Page();
            }

            try
            {
                // Verificar si ya existe un cliente con el mismo número de documento
                if (await _clienteService.ExistsByNumeroDocumentoAsync(Cliente.NumeroDocumento))
                {
                    ModelState.AddModelError("Cliente.NumeroDocumento", "Ya existe un cliente con este DNI.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        Response.StatusCode = 422;
                        return Partial("_CreatePartial", this);
                    }
                    return Page();
                }

                var result = await _clienteService.CreateClienteAsync(Cliente);
                _logger.LogInformation("Cliente creado exitosamente: {ClienteId} - {Nombre} {Apellido}", 
                    result.ClienteID, result.Nombre, result.Apellido);
                
                TempData["SuccessMessage"] = "Cliente creado exitosamente.";
                
                // Si es una petición AJAX, devolver JSON con éxito
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = true, message = "Cliente creado exitosamente" });
                }
                
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear cliente");
                ModelState.AddModelError(string.Empty, ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 422;
                    return Partial("_CreatePartial", this);
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear cliente");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el cliente. Por favor, intente nuevamente.");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 422;
                    return Partial("_CreatePartial", this);
                }
                return Page();
            }
        }
    }
}