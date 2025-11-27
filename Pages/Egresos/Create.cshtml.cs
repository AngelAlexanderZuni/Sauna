using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Services;
using System.Security.Claims;

namespace ProyectoSaunaKalixto.Web.Pages.Egresos
{
    [Authorize(Roles = "Administrador,Admin")]
    public class CreateModel : PageModel
    {
        private readonly EgresoService _egresoService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(EgresoService egresoService, ILogger<CreateModel> logger)
        {
            _egresoService = egresoService;
            _logger = logger;
        }

        [BindProperty]
        public EgresoCreateDTO Egreso { get; set; } = new EgresoCreateDTO();

        public List<TipoEgreso> TiposEgreso { get; set; } = new List<TipoEgreso>();

        public async Task OnGetAsync()
        {
            TiposEgreso = await _egresoService.ObtenerTiposEgresoAsync();
        }

        public async Task<IActionResult> OnGetModalAsync()
        {
            TiposEgreso = await _egresoService.ObtenerTiposEgresoAsync();
            _logger.LogInformation("📝 Modal crear egreso abierto. Tipos disponibles: {Count}", TiposEgreso.Count);
            return Partial("_CreatePartial", this);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("📥 Recibiendo petición POST para crear egreso");
            _logger.LogInformation("📋 Content-Type: {ContentType}", Request.ContentType);
            _logger.LogInformation("📏 Content-Length: {Length}", Request.ContentLength);
            
            // Leer el body directamente para debugging
            Request.EnableBuffering();
            using (var reader = new StreamReader(Request.Body, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                _logger.LogInformation("📦 Body RAW: {Body}", body);
                Request.Body.Position = 0; // Reset para que el model binder pueda leerlo
            }
            
            // Intentar deserializar manualmente
            EgresoCreateDTO? dto = null;
            try
            {
                Request.Body.Position = 0;
                dto = await System.Text.Json.JsonSerializer.DeserializeAsync<EgresoCreateDTO>(
                    Request.Body, 
                    new System.Text.Json.JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                _logger.LogInformation("✅ Deserialización manual exitosa");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en deserialización manual");
            }
            
            _logger.LogInformation("📄 DTO es null: {IsNull}", dto == null);
            
            if (dto != null)
            {
                _logger.LogInformation("📅 Fecha recibida: {Fecha}", dto.Fecha);
                _logger.LogInformation("📊 Detalles count: {Count}", dto.Detalles?.Count ?? 0);
                
                if (dto.Detalles != null && dto.Detalles.Any())
                {
                    for (int i = 0; i < dto.Detalles.Count; i++)
                    {
                        var detalle = dto.Detalles[i];
                        _logger.LogInformation("   Detalle {Index}: Concepto={Concepto}, Monto={Monto}, TipoId={TipoId}", 
                            i, detalle.Concepto, detalle.Monto, detalle.IdTipoEgreso);
                    }
                }
            }
            
            _logger.LogInformation("✅ ModelState válido: {IsValid}", ModelState.IsValid);
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                    
                _logger.LogWarning("❌ Errores de validación: {Errors}", string.Join(", ", errors));
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "Errores de validación: " + string.Join(", ", errors),
                        errors = errors
                    }) { StatusCode = 422 };
                }
                return Page();
            }

            try
            {
                var userIdClaim = User.FindFirstValue("UserId");
                if (!int.TryParse(userIdClaim, out int idUsuario))
                {
                    _logger.LogWarning("Usuario no autenticado intentó crear egreso");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return new JsonResult(new { success = false, message = "Usuario no identificado" })
                        { StatusCode = 401 };
                    }
                    
                    ModelState.AddModelError(string.Empty, "Usuario no identificado");
                    return Page();
                }

                var resultado = await _egresoService.CrearEgresoAsync(dto, idUsuario);
                
                _logger.LogInformation("Egreso creado exitosamente: {EgresoId} - Total: S/ {Total:N2}", 
                    resultado.IdCabEgreso, resultado.MontoTotal);

                TempData["SuccessMessage"] = $"Egreso registrado correctamente. Total: S/ {resultado.MontoTotal:N2}";

                // Si es una petición AJAX, devolver JSON con éxito
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new 
                    { 
                        success = true, 
                        message = $"Egreso registrado correctamente. Total: S/ {resultado.MontoTotal:N2}",
                        data = resultado
                    });
                }

                return RedirectToPage("./Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear egreso");
                ModelState.AddModelError(string.Empty, ex.Message);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 422;
                    TiposEgreso = await _egresoService.ObtenerTiposEgresoAsync();
                    return Partial("_CreatePartial", this);
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear egreso");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el egreso. Por favor, intente nuevamente.");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return new JsonResult(new { success = false, message = ex.Message })
                    { StatusCode = 400 };
                }
                return Page();
            }
        }
    }
}
