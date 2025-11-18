using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Clientes
{
    [Authorize(Policy = "RequireClienteAccess")]
    public class EditModel : PageModel
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IClienteService clienteService, ILogger<EditModel> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        [BindProperty]
        public ClienteEditDTO Cliente { get; set; } = new ClienteEditDTO();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByIdAsync(id);
                
                if (cliente == null)
                {
                    return NotFound();
                }

                Cliente = new ClienteEditDTO
                {
                    ClienteID = cliente.ClienteID,
                    NumeroDocumento = cliente.NumeroDocumento,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Telefono = cliente.Telefono,
                    Correo = cliente.Correo,
                    Direccion = cliente.Direccion,
                    FechaNacimiento = cliente.FechaNacimiento,
                    Activo = cliente.Activo
                };

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cliente para edición");
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

                Cliente = new ClienteEditDTO
                {
                    ClienteID = cliente.ClienteID,
                    NumeroDocumento = cliente.NumeroDocumento,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Telefono = cliente.Telefono,
                    Correo = cliente.Correo,
                    Direccion = cliente.Direccion,
                    FechaNacimiento = cliente.FechaNacimiento,
                    Activo = cliente.Activo
                };

                return Partial("_EditPartial", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar cliente para edición");
                return Content("Error al cargar el formulario");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 422;
                    return Partial("_EditPartial", this);
                }
                return Page();
            }

            try
            {
                var updatedCliente = await _clienteService.UpdateClienteAsync(Cliente.ClienteID, Cliente);
                
                if (updatedCliente == null)
                {
                    ModelState.AddModelError(string.Empty, "No se encontró el cliente.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        Response.StatusCode = 422;
                        return Partial("_EditPartial", this);
                    }
                    return NotFound();
                }

                _logger.LogInformation("Cliente actualizado exitosamente: {ClienteId} - {Nombre} {Apellido}", 
                    updatedCliente.ClienteID, updatedCliente.Nombre, updatedCliente.Apellido);

                TempData["SuccessMessage"] = "Cliente actualizado exitosamente.";
                
                // Si es una petición AJAX, devolver JSON con éxito
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = true, message = "Cliente actualizado exitosamente" });
                }
                
                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar cliente");
                ModelState.AddModelError(string.Empty, ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 422;
                    return Partial("_EditPartial", this);
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar cliente");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el cliente. Por favor, intente nuevamente.");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 422;
                    return Partial("_EditPartial", this);
                }
                return Page();
            }
        }
        
        public async Task<IActionResult> OnPostReactivateAsync(int id)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByIdAsync(id);
                
                if (cliente == null)
                {
                    return NotFound();
                }

                // Reactivar el cliente
                var clienteEdit = new ClienteEditDTO
                {
                    ClienteID = cliente.ClienteID,
                    NumeroDocumento = cliente.NumeroDocumento,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Telefono = cliente.Telefono,
                    Correo = cliente.Correo,
                    Direccion = cliente.Direccion,
                    FechaNacimiento = cliente.FechaNacimiento, // Ya no es nullable
                    Activo = true // REACTIVAR
                };
                
                var updatedCliente = await _clienteService.UpdateClienteAsync(id, clienteEdit);
                
                if (updatedCliente == null)
                {
                    return NotFound();
                }

                _logger.LogInformation("Cliente reactivado exitosamente: {ClienteId} - {Nombre} {Apellido}", 
                    updatedCliente.ClienteID, updatedCliente.Nombre, updatedCliente.Apellido);

                TempData["SuccessMessage"] = "Cliente reactivado exitosamente.";
                return new JsonResult(new { success = true, message = "Cliente reactivado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reactivar cliente: {ClienteId}", id);
                return new JsonResult(new { success = false, message = "Error al reactivar el cliente: " + ex.Message });
            }
        }
    }
}